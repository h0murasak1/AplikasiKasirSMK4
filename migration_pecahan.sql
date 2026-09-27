-- ============================================================
-- MIGRASI: Mengubah kolom nominal menjadi pecahan (DECIMAL)
-- ============================================================
-- Tujuan  : mengizinkan harga, total, dan diskon bernilai pecahan
--            (mis. 2500,75) tanpa kehilangan angka bulat.
-- Target  : MariaDB 10.4 / MySQL 5.7 ke atas
--
-- CATATAN PENTING
--   Kolom INT yang sudah terisi akan dikonversi otomatis,
--   contoh: 2500 (int) -> 2500.00 (decimal). Tidak ada data hilang.
--
--   KolomQty dan Stok SENGAJA dibiarkan INT.
--   Bila Anda ingin menjual barang timbang (mis. 0,5 kg),
--   jalankan skrip pada bagian "OPSIONAL" di bawah.
--
-- Sebelum menjalankan: pastikan database sudah di-backup.
-- ============================================================

USE db_kasir_smk4;

-- ------------------------------------------------------------
-- 1. Tabel Barang: harga beli, harga jual, harga grosir
-- ------------------------------------------------------------
ALTER TABLE tb_barang
  MODIFY COLUMN harga_beli   DECIMAL(15,2) NOT NULL DEFAULT 0.00,
  MODIFY COLUMN harga_jual   DECIMAL(15,2) NOT NULL DEFAULT 0.00,
  MODIFY COLUMN harga_grosir DECIMAL(15,2) NULL     DEFAULT 0.00;

-- ------------------------------------------------------------
-- 2. Tabel Transaksi: total bayar dan diskon total
-- ------------------------------------------------------------
ALTER TABLE tb_transaksi
  MODIFY COLUMN total_bayar DECIMAL(15,2) NOT NULL DEFAULT 0.00,
  MODIFY COLUMN diskon_total DECIMAL(15,2) NULL    DEFAULT 0.00;

-- ------------------------------------------------------------
-- 3. Tabel Detail Transaksi: subtotal dan diskon per item
-- ------------------------------------------------------------
ALTER TABLE tb_detail_transaksi
  MODIFY COLUMN subtotal    DECIMAL(15,2) NOT NULL DEFAULT 0.00,
  MODIFY COLUMN diskon_item DECIMAL(15,2) NULL    DEFAULT 0.00;

-- ============================================================
-- OPSIONAL: qty dan stok pecahan (barang timbang)
-- ============================================================
-- Jangan langsung dijalankan. Jalankan hanya bila aplikasi Anda
-- menjual barang timbang, misalnya 0,5 kg beras.
-- Perlu penyesuaian kode C# karena qty saat ini memakai tipe int.
--
-- ALTER TABLE tb_barang
--   MODIFY COLUMN stok          DECIMAL(15,2) NOT NULL DEFAULT 0.00,
--   MODIFY COLUMN minimal_grosir DECIMAL(15,2) NULL     DEFAULT 0.00;
--
-- ALTER TABLE tb_detail_transaksi
--   MODIFY COLUMN qty DECIMAL(15,2) NOT NULL DEFAULT 0.00;

-- ============================================================
-- VERIFIKASI: pastikan semua kolom sudah DECIMAL
-- ============================================================
SELECT TABLE_NAME, COLUMN_NAME, COLUMN_TYPE, IS_NULLABLE, COLUMN_DEFAULT
FROM information_schema.COLUMNS
WHERE TABLE_SCHEMA = 'db_kasir_smk4'
  AND COLUMN_NAME IN (
    'harga_beli', 'harga_jual', 'harga_grosir',
    'total_bayar', 'diskon_total',
    'subtotal', 'diskon_item'
  )
ORDER BY TABLE_NAME, ORDINAL_POSITION;
