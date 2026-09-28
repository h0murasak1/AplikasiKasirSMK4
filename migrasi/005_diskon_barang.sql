-- ===================================================================
--  MIGRASI 005 - DISKON BARANG
--  Dijalankan oleh Database.cs pada database yang sudah versi 4.
-- ===================================================================

-- Tambah kolom diskon_persen pada tabel master tb_barang
ALTER TABLE tb_barang ADD COLUMN diskon_persen NUMERIC NOT NULL DEFAULT 0;
