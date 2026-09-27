-- ============================================================================
-- SKEMA DATABASE APLIKASI KASIR SMK NEGERI 4
-- ============================================================================
-- Versi   : 3.0 (setelah restrukturasi)
-- Target  : MariaDB 10.4+ / MySQL 8.0+
-- Encoding: utf8mb4
--
-- CARA PAKAI
--   Untuk PASANG BARU : jalankan berkas ini dari awal.
--   Untuk MIGRASI dari versi lama (1.0/2.0) : jalankan migration_restruktur.sql
--   lalu migration_id_unsigned.sql. Jangan jalankan berkas ini, karena akan
--   gagal dengan pesan tabel sudah ada.
--
--   Di phpMyAdmin  : menu Import > pilih berkas ini > Go
--   Di PowerShell  : cmd /c "C:\xampp\mysql\bin\mysql.exe -u root < skema_database.sql"
--                   (PowerShell tidak mendukung operator <)
--
-- RINGKASAN PERUBAHAN DARI VERSI LAMA
--   + 5 FOREIGN KEY (sebelumnya 0) -> integritas data dijamin database
--   + 11 CHECK constraint -> nilai negatif / tidak logis ditolak
--   + 11 INDEX -> laporan tidak melambat seiring data bertambah
--   + kolom id_transaksi -> primary key numerik, no_nota jadi UNIQUE
--   + snapshot nama_barang & harga_satuan -> nota lama tidak berubah
--   + uang_diterima & kembalian -> bisa buat laporan rekonsiliasi kas
--   + is_active -> hapus barang jadi hapus logis, riwayat tetap utuh
--   + tabel tb_mutasi_stok -> bisa diaudit kenapa stok berubah
--   + kolom satuan (pcs/kg/box)
--   + qty & stok jadi DECIMAL(15,2) -> bisa jual barang timbang
--   + timestamps dibuat_pada / diperbarui_pada
--   ~ role dari ENUM jadi VARCHAR(20) + CHECK
-- ============================================================================

CREATE DATABASE IF NOT EXISTS db_kasir_smk4
  DEFAULT CHARACTER SET utf8mb4
  DEFAULT COLLATE utf8mb4_general_ci;

USE db_kasir_smk4;

-- ============================================================================
-- 1. USERS
-- ============================================================================
-- role hanya boleh 'Admin' atau 'Kasir'. Daftar ini WAJIB sama dengan
-- SessionIsRoleValid() di Form1.cs, kalau tidak setiap role baru
-- harus diubah di dua tempat.
-- is_active dipakai untuk menonaktifkan akun tanpa menghapus riwayat
-- transaksinya. Akun nonaktif tetap tercatat di tb_transaksi.
-- ============================================================================
CREATE TABLE IF NOT EXISTS tb_user (
  id_user        INT UNSIGNED     NOT NULL AUTO_INCREMENT,
  nama_lengkap   VARCHAR(100)     NOT NULL,
  username       VARCHAR(50)      NOT NULL,
  password       VARCHAR(255)     NOT NULL COMMENT 'PBKDF2-SHA256; password lama (plain text) tetap bisa login lalu di-upgrade otomatis',
  role           VARCHAR(20)      NOT NULL DEFAULT 'Kasir',
  is_active      TINYINT(1)       NOT NULL DEFAULT 1,
  dibuat_pada     TIMESTAMP        NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id_user),
  UNIQUE KEY uq_user_username (username),
  KEY ix_user_aktif (is_active, role),
  CONSTRAINT chk_user_role CHECK (role IN ('Admin', 'Kasir'))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ============================================================================
-- 2. BARANG / INVENTORI
-- ============================================================================
-- is_active = 1 aktif, 0 dinonaktifkan (soft delete).
-- Barang yang dinonaktifkan disembunyikan dari daftar dan tidak bisa dijual,
-- tapi barisnya tetap ada agar riwayat transaksi lama tidak rusak.
--
-- Diskon grosir (minimal_grosir + harga_grosir) sudah disiapkan di skema
-- tapi belum dipakai kode aplikasi. Kolomnya tidak mengganggu.
-- ============================================================================
CREATE TABLE IF NOT EXISTS tb_barang (
  kode_barcode    VARCHAR(50)     NOT NULL,
  nama_barang     VARCHAR(150)    NOT NULL,
  satuan          VARCHAR(20)     NOT NULL DEFAULT 'pcs' COMMENT 'pcs / kg / box / lusin',
  harga_beli      DECIMAL(15,2)   NOT NULL DEFAULT 0.00,
  harga_jual      DECIMAL(15,2)   NOT NULL DEFAULT 0.00,
  stok            DECIMAL(15,2)   NOT NULL DEFAULT 0.00 COMMENT 'boleh pecahan, mis. 12.75 kg',
  minimal_grosir  DECIMAL(15,2)   NOT NULL DEFAULT 0.00,
  harga_grosir    DECIMAL(15,2)   NOT NULL DEFAULT 0.00,
  is_active       TINYINT(1)      NOT NULL DEFAULT 1,
  dibuat_pada     TIMESTAMP       NOT NULL DEFAULT CURRENT_TIMESTAMP,
  diperbarui_pada TIMESTAMP       NOT NULL DEFAULT CURRENT_TIMESTAMP
                                       ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (kode_barcode),
  KEY ix_barang_nama  (nama_barang),
  KEY ix_barang_aktif (is_active, nama_barang),
  CONSTRAINT chk_barang_harga_beli CHECK (harga_beli >= 0),
  CONSTRAINT chk_barang_harga_jual CHECK (harga_jual > 0),
  CONSTRAINT chk_barang_stok       CHECK (stok >= 0),
  CONSTRAINT chk_barang_grosir     CHECK (minimal_grosir >= 0
                                       AND harga_grosir >= 0)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ============================================================================
-- 3. TRANSAKSI (HEADER / NOTA)
-- ============================================================================
-- id_transaksi = primary key numerik yang dipakai foreign key.
-- no_nota tetap disimpan karena itu nomor yang dilihat manusia (dicetak di
-- struk) dan hanya perlu UNIQUE, bukan primary key varchar.
--
-- uang_diterima & kembalian WAJIB disimpan. Sebelumnya aplikasi menghitung
-- kembalian di layar lalu membuangnya, sehingga tidak ada cara knows berapa
-- uang yang benar-benar diterima untuk rekonsiliasi kas akhir hari.
-- ============================================================================
CREATE TABLE IF NOT EXISTS tb_transaksi (
  id_transaksi   BIGINT UNSIGNED  NOT NULL AUTO_INCREMENT,
  no_nota        VARCHAR(50)      NOT NULL,
  tanggal        DATETIME         NOT NULL DEFAULT CURRENT_TIMESTAMP,
  id_user        INT UNSIGNED     NOT NULL,
  total_bayar    DECIMAL(15,2)    NOT NULL DEFAULT 0.00,
  diskon_total   DECIMAL(15,2)    NOT NULL DEFAULT 0.00,
  uang_diterima  DECIMAL(15,2)    NOT NULL DEFAULT 0.00,
  kembalian      DECIMAL(15,2)    NOT NULL DEFAULT 0.00,
  catatan        VARCHAR(255)     NULL,
  PRIMARY KEY (id_transaksi),
  UNIQUE KEY uq_transaksi_no_nota (no_nota),
  KEY ix_transaksi_tanggal (tanggal),
  KEY ix_transaksi_user    (id_user, tanggal),
  CONSTRAINT fk_transaksi_user FOREIGN KEY (id_user)
    REFERENCES tb_user (id_user)
    ON UPDATE CASCADE ON DELETE RESTRICT,
  CONSTRAINT chk_transaksi_total    CHECK (total_bayar   >= 0),
  CONSTRAINT chk_transaksi_diterima CHECK (uang_diterima >= 0),
  CONSTRAINT chk_transaksi_kembali  CHECK (kembalian     >= 0),
  CONSTRAINT chk_transaksi_diskon   CHECK (diskon_total  >= 0)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ============================================================================
-- 4. DETAIL TRANSAKSI (BARANG YANG DIBELI)
-- ============================================================================
-- PENTING: kolom nama_barang dan harga_satuan adalah SNAPSHOT, bukan salinan.
-- Nilainya ditulis ulang pada saat penjualan dan sengaja TIDAK ikut berubah
-- ketika master barang diperbarui. Tujuannya supaya nota lama masih
-- menampilkan nama & harga yang benar pada saat penjualan, walaupun barangnya
-- sekarang sudah diganti nama atau harganya sudah dinaikkan.
--
-- no_nota sengaja disimpan dua kali (sekali sebagai no_nota, sekali lewat
-- id_transaksi) supaya laporan per nota tidak perlu JOIN dulu.
-- ============================================================================
CREATE TABLE IF NOT EXISTS tb_detail_transaksi (
  id_detail       BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  id_transaksi    BIGINT UNSIGNED NOT NULL,
  no_nota         VARCHAR(50)     NOT NULL,
  kode_barcode    VARCHAR(50)     NOT NULL,
  nama_barang     VARCHAR(150)    NOT NULL COMMENT 'snapshot nama saat penjualan',
  qty             DECIMAL(15,2)   NOT NULL,
  harga_satuan    DECIMAL(15,2)   NOT NULL COMMENT 'snapshot harga saat penjualan',
  subtotal        DECIMAL(15,2)   NOT NULL DEFAULT 0.00,
  diskon_item     DECIMAL(15,2)   NOT NULL DEFAULT 0.00,
  PRIMARY KEY (id_detail),
  KEY ix_detail_nota   (no_nota),
  KEY ix_detail_barang (kode_barcode),
  CONSTRAINT fk_detail_transaksi FOREIGN KEY (id_transaksi)
    REFERENCES tb_transaksi (id_transaksi)
    ON UPDATE CASCADE ON DELETE CASCADE,
  CONSTRAINT fk_detail_barang FOREIGN KEY (kode_barcode)
    REFERENCES tb_barang (kode_barcode)
    ON UPDATE CASCADE ON DELETE RESTRICT,
  CONSTRAINT chk_detail_qty          CHECK (qty > 0),
  CONSTRAINT chk_detail_harga_satuan CHECK (harga_satuan >= 0),
  CONSTRAINT chk_detail_subtotal     CHECK (subtotal >= 0),
  CONSTRAINT chk_detail_diskon       CHECK (diskon_item >= 0)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ============================================================================
-- 5. MUTASI STOK (RIWAYAT PERUBAHAN STOK)
-- ============================================================================
-- Tabel ini menjawab pertanyaan: "kenapa stok barang A berubah?"
--
-- tipe  MASUK       = barang baru / restok / barang diaktifkan kembali
-- tipe  KELUAR      = terjual di kasir
-- tipe  PENYESUAIAN = koreksi admin, mis. hasil opname atau barang rusak
--
-- qty bernilai bertanda: positif = stok bertambah, negatif = stok berkurang.
-- stok_akhir menyimpan snapshot stok setelah perubahan, jadi bisa langsung
-- dibandingkan dengan stok sekarang tanpa menghitung ulang.
--
-- Tabel ini diisi oleh APLIKASI, bukan trigger, supaya logikanya terlihat
-- jelas di kode C# dan mudah ditelusuri saat debugging.
-- ============================================================================
CREATE TABLE IF NOT EXISTS tb_mutasi_stok (
  id_mutasi     BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  tanggal       DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
  kode_barcode  VARCHAR(50)     NOT NULL,
  tipe          ENUM('MASUK','KELUAR','PENYESUAIAN') NOT NULL,
  qty             DECIMAL(15,2)   NOT NULL COMMENT 'positif = stok masuk, negatif = stok keluar',
  stok_akhir    DECIMAL(15,2)   NOT NULL COMMENT 'stok setelah perubahan ini',
  keterangan    VARCHAR(255)    NULL,
  id_user       INT UNSIGNED    NULL COMMENT 'siapa yang melakukan; NULL untuk data rekonstruksi',
  ref_no_nota   VARCHAR(50)     NULL COMMENT 'nomor nota kalau berasal dari penjualan',
  PRIMARY KEY (id_mutasi),
  KEY ix_mutasi_barang  (kode_barcode, tanggal),
  KEY ix_mutasi_tanggal (tanggal),
  KEY ix_mutasi_tipe    (tipe),
  CONSTRAINT fk_mutasi_barang FOREIGN KEY (kode_barcode)
    REFERENCES tb_barang (kode_barcode)
    ON UPDATE CASCADE ON DELETE RESTRICT,
  CONSTRAINT fk_mutasi_user FOREIGN KEY (id_user)
    REFERENCES tb_user (id_user)
    ON UPDATE CASCADE ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ============================================================================
-- 6. VIEW LAPORAN
-- ============================================================================
-- Dua view ini tidak dipakai kode aplikasi, tapi berguna saat
-- membuat laporan baru dengan cukup satu SELECT.
-- ============================================================================

-- Penjualan per baris detail, sudah termasuk nama kasir.
CREATE OR REPLACE VIEW v_laporan_penjualan AS
SELECT
  t.no_nota,
  t.tanggal,
  u.username,
  u.nama_lengkap,
  d.kode_barcode,
  d.nama_barang,
  d.qty,
  d.harga_satuan,
  d.diskon_item,
  d.subtotal
FROM tb_transaksi t
JOIN tb_user u              ON t.id_user      = u.id_user
JOIN tb_detail_transaksi d ON d.id_transaksi = t.id_transaksi;

-- Persediaan beserta nilai uangnya (stok x harga beli).
CREATE OR REPLACE VIEW v_stok_gudang AS
SELECT
  b.kode_barcode,
  b.nama_barang,
  b.satuan,
  b.harga_beli,
  b.harga_jual,
  b.stok,
  (b.stok * b.harga_beli) AS nilai_persediaan,
  b.is_active
FROM tb_barang b;

-- ============================================================================
-- 7. DATA AWAL
-- ============================================================================
-- Password masih plain text "admin123". Aplikasi akan meng-hash-nya
-- otomatis (PBKDF2-SHA256) begitu login pertama berhasil, jadi tidak
-- perlu reset manual.
--
-- GANTI password ini sebelum dipakai di lingkungan nyata.
-- ============================================================================
INSERT INTO tb_user (nama_lengkap, username, password, role)
VALUES
  ('Administrator Sekolah', 'admin', 'admin123', 'Admin'),
  ('Kasir Toko', 'kasir', 'kasir123', 'Kasir')
ON DUPLICATE KEY UPDATE nama_lengkap = VALUES(nama_lengkap);

-- Contoh barang, termasuk satu barang timbang satuan kg.
INSERT INTO tb_barang
  (kode_barcode, nama_barang, satuan, harga_beli, harga_jual, stok)
VALUES
  ('123', 'Buku Tulis',      'pcs', 5000.00,  7500.00,  99.00),
  ('124', 'Mie Goreng',      'pcs', 2500.00,  3500.00,  99.00),
  ('125', 'Beras Premium 5kg','kg',55000.00, 65000.50,  12.75)
ON DUPLICATE KEY UPDATE nama_barang = VALUES(nama_barang);

SELECT 'Skema database siap digunakan' AS status, NOW() AS waktu;
