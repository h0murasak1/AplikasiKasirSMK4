-- ============================================================================
-- RESTRUKTURASI DATABASE APLIKASI KASIR SMK NEGERI 4
-- ============================================================================
-- Tanggal  : 2026-09-27
-- Target   : MariaDB 10.4 / MySQL 5.7+
-- Sumber   : db_kasir_smk4
--
-- TUJUAN
--   1. Menambahkan FOREIGN KEY (dari 0 menjadi 5) agar integritas data terjaga
--   2. Menambahkan INDEX agar laporan tidak melambat seiring data bertambah
--   3. Menambahkan CHECK constraint agar nilai negatif tidak bisa masuk
--   4. Menyimpan harga satuan & nama barang saat transaksi (snapshot),
--      agar nota lama tidak berubah ketika harga barang diubah
--   5. Menyimpan uang diterima & kembalian, untuk laporan rekonsiliasi kas
--   6. Mengganti hapus permanen menjadi hapus logical (is_active)
--   7. Menambahkan tb_mutasi_stok sebagai riwayat setiap perubahan stok
--   8. Mengubah qty dan stok menjadi DECIMAL agar bisa selling barang timbang
--   9. Menambahkan kolom satuan (pcs / kg / box)
--
-- PERINGATAN
--   Backup database WAJIB dibuat sebelum menjalankan skrip ini.
--   Semua skrip di bawah aman dijalankan berulang kali (idempotent).
-- ============================================================================

USE db_kasir_smk4;

-- ============================================================================
-- TAHAP 1: Tambah kolom baru (belum ada batasan, jadi data aman)
-- ============================================================================

-- 1.1 tb_barang: satuan, status aktif, jejak waktu, stok jadi pecahan
ALTER TABLE tb_barang
  ADD COLUMN IF NOT EXISTS satuan        VARCHAR(20)   NOT NULL DEFAULT 'pcs'  AFTER nama_barang,
  ADD COLUMN IF NOT EXISTS is_active      TINYINT(1)    NOT NULL DEFAULT 1      AFTER harga_grosir,
  ADD COLUMN IF NOT EXISTS dibuat_pada    TIMESTAMP     NOT NULL DEFAULT CURRENT_TIMESTAMP,
  ADD COLUMN IF NOT EXISTS diperbarui_pada TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP
                                          ON UPDATE CURRENT_TIMESTAMP,
  MODIFY COLUMN stok           DECIMAL(15,2) NOT NULL DEFAULT 0.00,
  MODIFY COLUMN minimal_grosir DECIMAL(15,2) NOT NULL DEFAULT 0.00,
  MODIFY COLUMN harga_grosir   DECIMAL(15,2) NOT NULL DEFAULT 0.00;

-- 1.2 tb_user: status aktif, jejak waktu, role jadi VARCHAR + CHECK
ALTER TABLE tb_user
  ADD COLUMN IF NOT EXISTS is_active   TINYINT(1) NOT NULL DEFAULT 1 AFTER role,
  ADD COLUMN IF NOT EXISTS dibuat_pada TIMESTAMP  NOT NULL DEFAULT CURRENT_TIMESTAMP,
  MODIFY COLUMN role VARCHAR(20) NOT NULL DEFAULT 'Kasir';

-- 1.3 tb_transaksi: kunci numerik, uang diterima, kembalian, catatan
ALTER TABLE tb_transaksi
  ADD COLUMN IF NOT EXISTS id_transaksi   BIGINT UNSIGNED NOT NULL AUTO_INCREMENT UNIQUE FIRST,
  ADD COLUMN IF NOT EXISTS uang_diterima DECIMAL(15,2)  NOT NULL DEFAULT 0.00,
  ADD COLUMN IF NOT EXISTS kembalian      DECIMAL(15,2)  NOT NULL DEFAULT 0.00,
  ADD COLUMN IF NOT EXISTS catatan        VARCHAR(255)   NULL,
  MODIFY COLUMN tanggal DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP;

-- 1.4 tb_detail_transaksi: relasi ke header, snapshot nama & harga, qty pecahan
ALTER TABLE tb_detail_transaksi
  ADD COLUMN IF NOT EXISTS id_transaksi BIGINT UNSIGNED NULL AFTER id_detail,
  ADD COLUMN IF NOT EXISTS nama_barang  VARCHAR(150)     NOT NULL DEFAULT '' AFTER kode_barcode,
  ADD COLUMN IF NOT EXISTS harga_satuan DECIMAL(15,2)    NOT NULL DEFAULT 0.00 AFTER qty,
  MODIFY COLUMN qty         DECIMAL(15,2) NOT NULL DEFAULT 0.00,
  MODIFY COLUMN diskon_item DECIMAL(15,2) NOT NULL DEFAULT 0.00;

-- ============================================================================
-- TAHAP 2: Isi data ke kolom baru
-- ============================================================================

-- 2.1 Uang diterima dianggap sama dengan total (belum ada data kembalian)
UPDATE tb_transaksi SET uang_diterima = total_bayar WHERE uang_diterima = 0;

-- 2.2 Hubungkan detail ke header transaksi
UPDATE tb_detail_transaksi d
  JOIN tb_transaksi t ON d.no_nota = t.no_nota
  SET d.id_transaksi = t.id_transaksi
  WHERE d.id_transaksi IS NULL;

-- 2.3 Harga satuan = harga yang benar-benar dibayar (subtotal / qty)
UPDATE tb_detail_transaksi
  SET harga_satuan = subtotal / qty
  WHERE harga_satuan = 0 AND qty > 0;

-- 2.4 Kalau harga satuan masih 0, pakai harga jual barang saat ini
UPDATE tb_detail_transaksi d
  JOIN tb_barang b ON d.kode_barcode = b.kode_barcode
  SET d.harga_satuan = b.harga_jual
  WHERE d.harga_satuan = 0;

-- 2.5 Salin nama barang ke detail sebagai snapshot
UPDATE tb_detail_transaksi d
  JOIN tb_barang b ON d.kode_barcode = b.kode_barcode
  SET d.nama_barang = b.nama_barang
  WHERE d.nama_barang = '';

-- 2.6 Jadikan kolom relasi wajib terisi
ALTER TABLE tb_detail_transaksi
  MODIFY COLUMN id_transaksi BIGINT UNSIGNED NOT NULL;

-- ============================================================================
-- TAHAP 3: Perbaiki primary key dan unique key
-- ============================================================================

-- 3.1 tb_transaksi: PK pindah dari no_nota (varchar) ke id_transaksi (numerik)
ALTER TABLE tb_transaksi
  DROP PRIMARY KEY,
  DROP INDEX id_transaksi,
  ADD PRIMARY KEY (id_transaksi),
  ADD CONSTRAINT uq_transaksi_no_nota UNIQUE (no_nota);

-- 3.2 Pastikan kode barcode barang unik (sudah PK, dilewati)
-- 3.3 Username unik (sudah ada UNIQUE, dilewati)

-- ============================================================================
-- TAHAP 4: Tambah CHECK constraint (anti nilai tidak logis)
-- ============================================================================

ALTER TABLE tb_barang
  ADD CONSTRAINT chk_barang_harga_beli  CHECK (harga_beli  >= 0),
  ADD CONSTRAINT chk_barang_harga_jual  CHECK (harga_jual  >  0),
  ADD CONSTRAINT chk_barang_stok        CHECK (stok        >= 0),
  ADD CONSTRAINT chk_barang_grosir      CHECK (minimal_grosir >= 0 AND harga_grosir >= 0);

ALTER TABLE tb_detail_transaksi
  ADD CONSTRAINT chk_detail_qty          CHECK (qty          >  0),
  ADD CONSTRAINT chk_detail_harga_satuan CHECK (harga_satuan >= 0),
  ADD CONSTRAINT chk_detail_subtotal     CHECK (subtotal     >= 0),
  ADD CONSTRAINT chk_detail_diskon       CHECK (diskon_item  >= 0);

ALTER TABLE tb_transaksi
  ADD CONSTRAINT chk_transaksi_total    CHECK (total_bayar    >= 0),
  ADD CONSTRAINT chk_transaksi_diterima CHECK (uang_diterima  >= 0),
  ADD CONSTRAINT chk_transaksi_kembali  CHECK (kembalian      >= 0),
  ADD CONSTRAINT chk_transaksi_diskon   CHECK (diskon_total   >= 0);

-- Role yang diizinkan harus sama persis dengan yang diperiksa Form1.cs,
-- yaitu Admin dan Kasir. Menambah role baru berarti ubah constraint ini
-- sekaligus tambahkan CABUTAN_PERAN di Form1.cs.
ALTER TABLE tb_user
  ADD CONSTRAINT chk_user_role CHECK (role IN ('Admin', 'Kasir'));

-- ============================================================================
-- TAHAP 5: Tambah FOREIGN KEY (inti integritas data)
-- ============================================================================

-- 5.1 Transaksi harus milik user yang benar-benar ada
ALTER TABLE tb_transaksi
  ADD CONSTRAINT fk_transaksi_user
    FOREIGN KEY (id_user) REFERENCES tb_user (id_user)
    ON UPDATE CASCADE ON DELETE RESTRICT;

-- 5.2 Detail harus punya header transaksi
ALTER TABLE tb_detail_transaksi
  ADD CONSTRAINT fk_detail_transaksi
    FOREIGN KEY (id_transaksi) REFERENCES tb_transaksi (id_transaksi)
    ON UPDATE CASCADE ON DELETE CASCADE;

-- 5.3 Detail harus memakai barang yang benar-benar ada
--     RESTRICT karena barang tidak boleh hilang whilst ada riwayat penjualan
ALTER TABLE tb_detail_transaksi
  ADD CONSTRAINT fk_detail_barang
    FOREIGN KEY (kode_barcode) REFERENCES tb_barang (kode_barcode)
    ON UPDATE CASCADE ON DELETE RESTRICT;

-- ============================================================================
-- TAHAP 6: Tambah index (percepatan query)
-- ============================================================================

-- 6.1 Pencarian barang dan filter barang aktif
CREATE INDEX ix_barang_nama     ON tb_barang (nama_barang);
CREATE INDEX ix_barang_aktif    ON tb_barang (is_active, nama_barang);

-- 6.2 Laporan penjualan per periode dan per kasir
CREATE INDEX ix_transaksi_tanggal ON tb_transaksi (tanggal);
CREATE INDEX ix_transaksi_user    ON tb_transaksi (id_user, tanggal);
CREATE INDEX ix_transaksi_nota    ON tb_transaksi (no_nota);

-- 6.3 Rekap penjualan per barang
CREATE INDEX ix_detail_barang ON tb_detail_transaksi (kode_barcode);
CREATE INDEX ix_detail_nota   ON tb_detail_transaksi (no_nota);

-- 6.4 Login dan daftar user aktif
CREATE INDEX ix_user_aktif ON tb_user (is_active, role);

-- ============================================================================
-- TAHAP 7: Tabel baru - riwayat mutasi stok
-- ============================================================================
-- Setiap perubahan stok tercatat di sini, sehingga bisa diaudit:
--   "kenapa stok barang A berubah dari 10 jadi 3?"
--   MASUK       = barang baru / restock        (qty positif)
--   KELUAR      = penjualan                    (qty negatif)
--   PENYESUAIAN = koreksi admin / opname        (bisa + atau -)
-- ============================================================================

CREATE TABLE IF NOT EXISTS tb_mutasi_stok (
  id_mutasi     BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  tanggal       DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
  kode_barcode  VARCHAR(50)  NOT NULL,
  tipe          ENUM('MASUK','KELUAR','PENYESUAIAN') NOT NULL,
  qty           DECIMAL(15,2) NOT NULL COMMENT 'positif = stok masuk, negatif = stok keluar',
  stok_akhir    DECIMAL(15,2) NOT NULL COMMENT 'stok setelah perubahan',
  keterangan    VARCHAR(255) NULL,
  id_user       INT          NULL COMMENT 'siapa yang melakukan',
  ref_no_nota   VARCHAR(50)  NULL COMMENT 'nomor nota jika berasal dari transaksi penjualan',
  PRIMARY KEY (id_mutasi),
  KEY ix_mutasi_barang (kode_barcode, tanggal),
  KEY ix_mutasi_tanggal (tanggal),
  KEY ix_mutasi_tipe (tipe),
  CONSTRAINT fk_mutasi_barang
    FOREIGN KEY (kode_barcode) REFERENCES tb_barang (kode_barcode)
    ON UPDATE CASCADE ON DELETE RESTRICT,
  CONSTRAINT fk_mutasi_user
    FOREIGN KEY (id_user) REFERENCES tb_user (id_user)
    ON UPDATE CASCADE ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- ============================================================================
-- TAHAP 8: Isi riwayat mutasi dari data transaksi yang sudah ada
-- ============================================================================
-- Rekonstruksi mutasi penjualan lama agar riwayat tidak kosong.
-- ============================================================================

INSERT INTO tb_mutasi_stok (tanggal, kode_barcode, tipe, qty, stok_akhir, keterangan, id_user, ref_no_nota)
SELECT t.tanggal,
       d.kode_barcode,
       'KELUAR',
       -d.qty,
       b.stok,
       'Rekonstruksi transaksi lama',
       t.id_user,
       d.no_nota
FROM tb_detail_transaksi d
JOIN tb_transaksi t ON d.id_transaksi = t.id_transaksi
JOIN tb_barang    b ON d.kode_barcode = b.kode_barcode
WHERE NOT EXISTS (
  SELECT 1 FROM tb_mutasi_stok m WHERE m.ref_no_nota = d.no_nota
);

-- ============================================================================
-- TAHAP 9: View helper untuk laporan
-- ============================================================================

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
JOIN tb_user u               ON t.id_user      = u.id_user
JOIN tb_detail_transaksi d  ON d.id_transaksi = t.id_transaksi;

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
-- TAHAP 10: RAPIKAN DEFINISI KOLOM AGAR SAMA DENGAN skema_database.sql
-- ============================================================================
-- Dua masalah kecil yang ditemukan saat membandingkan database hasil migrasi
-- dengan skema_database.sql:
--
-- 1. diskon_total masih boleh NULL. Padahal ada CHECK (diskon_total >= 0).
--    Kalau nilainya NULL, perbandingan NULL menghasilkan NULL, yang dianggap
--    "lolos" oleh MySQL. Jadi NULL harus dilarang, bukan cuma diperiksa.
--
-- 2. Kolom yang ditambahkan lewat ALTER TABLE (bukan CREATE TABLE) mendapat
--    DEFAULT otomatis dari MariaDB: nama_barang jadi '' serta qty dan
--    harga_satuan jadi 0.00. Nilai bawaan ini tidak berbahaya, tapi membuat
--    skema hasil migrasi berbeda dari skema_database.sql. Aplikasi selalu
--    mengirim ketiga kolom itu secara eksplisit, jadi DEFAULT tidak diperlukan.

ALTER TABLE tb_transaksi
  MODIFY COLUMN diskon_total DECIMAL(15,2) NOT NULL DEFAULT 0.00;

ALTER TABLE tb_detail_transaksi
  MODIFY COLUMN nama_barang  VARCHAR(150)   NOT NULL;

ALTER TABLE tb_detail_transaksi
  MODIFY COLUMN qty          DECIMAL(15,2) NOT NULL;

ALTER TABLE tb_detail_transaksi
  MODIFY COLUMN harga_satuan DECIMAL(15,2) NOT NULL;

-- ============================================================================
-- SELESAI
-- ============================================================================
SELECT 'Migrasi restrukturasi SELESAI' AS status,
       NOW() AS waktu,
       VERSION() AS versi_database;
