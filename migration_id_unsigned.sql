-- Menyamakan tipe kolom ID menjadi UNSIGNED di seluruh tabel
-- Tujuannya: ID tidak mungkin bernilai negatif dan konsisten antar tabel.
--
-- Urutan wajib: drop FK -> ubah kolom induk -> ubah kolom anak -> buat ulang FK.
-- Foreign key tidak boleh diubah selama masih terhubung.

USE db_kasir_smk4;

-- 1. Lepaskan semua foreign key yang menyentuh kolom ID
ALTER TABLE tb_mutasi_stok       DROP FOREIGN KEY fk_mutasi_user;
ALTER TABLE tb_transaksi         DROP FOREIGN KEY fk_transaksi_user;
ALTER TABLE tb_detail_transaksi DROP FOREIGN KEY fk_detail_transaksi;

-- 2. Ubah kolom induk lebih dulu
ALTER TABLE tb_user
  MODIFY COLUMN id_user INT UNSIGNED NOT NULL AUTO_INCREMENT;

-- 3. Baru ubah kolom anak
ALTER TABLE tb_mutasi_stok
  MODIFY COLUMN id_user INT UNSIGNED NULL;

ALTER TABLE tb_transaksi
  MODIFY COLUMN id_user INT UNSIGNED NOT NULL;

-- 4. Samakan id_detail dengan id_transaksi
ALTER TABLE tb_detail_transaksi
  MODIFY COLUMN id_detail BIGINT UNSIGNED NOT NULL AUTO_INCREMENT;

-- 5. Pasang kembali foreign key
ALTER TABLE tb_transaksi
  ADD CONSTRAINT fk_transaksi_user
    FOREIGN KEY (id_user) REFERENCES tb_user (id_user)
    ON UPDATE CASCADE ON DELETE RESTRICT;

-- 6. Rapikan index yang menjadi kembar.
-- Saat no_nota diubah dari PRIMARY KEY menjadi UNIQUE, MySQL/MariaDB menyisakan
-- index lama (ix_transaksi_nota) yang isinya sama persis dengan
-- uq_transaksi_no_nota. Index kembar hanya memperlambat setiap INSERT.
ALTER TABLE tb_transaksi DROP INDEX ix_transaksi_nota;

-- Samakan nama index username supaya sama dengan skema_database.sql.
ALTER TABLE tb_user
  DROP INDEX username,
  ADD UNIQUE KEY uq_user_username (username);

ALTER TABLE tb_mutasi_stok
  ADD CONSTRAINT fk_mutasi_user
    FOREIGN KEY (id_user) REFERENCES tb_user (id_user)
    ON UPDATE CASCADE ON DELETE SET NULL;

ALTER TABLE tb_detail_transaksi
  ADD CONSTRAINT fk_detail_transaksi
    FOREIGN KEY (id_transaksi) REFERENCES tb_transaksi (id_transaksi)
    ON UPDATE CASCADE ON DELETE CASCADE;

SELECT 'Tipe ID sudah diseragamkan' AS status;