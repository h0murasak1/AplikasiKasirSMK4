-- ===================================================================
--  MIGRASI 003 - TAHAP 3
--  Dijalankan oleh Database.cs pada database yang sudah versi 2.
--
--  SELURUH BERKAS INI HANYA MENGANDUNG KARAKTER ASCII.
--
--  Yang ditambahkan:
--  - tb_poin_log : riwayat penambahan/penggunaan poin member
-- ===================================================================


-- -------------------------------------------------------------------
--  1. tb_poin_log  (riwayat poin member)
-- -------------------------------------------------------------------
--  Saldo poin TIDAK disimpan di tb_member. Yang disimpan adalah
--  peristiwa penambahan/pengurangan, dan saldo dihitung dari SUM.
--  Pola yang sama seperti tb_piutang_bayar.
--
--  tipe: TAMBAH  = poin masuk dari transaksi
--         PAKAI  = poin dipakai sebagai potongan
--         HANGUS = poin kadaluarsa atau dibatalkan admin
--
--  id_transaksi boleh NULL untuk penyesuaian manual oleh admin.
CREATE TABLE IF NOT EXISTS tb_poin_log
(
    id_log          INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    id_member       INTEGER NOT NULL,
    tanggal         TEXT    NOT NULL
                    DEFAULT (strftime('%Y-%m-%d %H:%M:%S', 'now', 'localtime')),
    tipe            TEXT    NOT NULL,
    jumlah_poin     INTEGER NOT NULL,
    id_transaksi    INTEGER,
    keterangan      TEXT,
    id_user         INTEGER,

    CONSTRAINT fk_poin_member      FOREIGN KEY (id_member)
        REFERENCES tb_member (id_member) ON DELETE CASCADE,
    CONSTRAINT fk_poin_transaksi   FOREIGN KEY (id_transaksi)
        REFERENCES tb_transaksi (id_transaksi) ON DELETE SET NULL,
    CONSTRAINT fk_poin_user        FOREIGN KEY (id_user)
        REFERENCES tb_user (id_user) ON DELETE SET NULL,
    CONSTRAINT chk_poin_tipe       CHECK (tipe IN ('TAMBAH', 'PAKAI', 'HANGUS')),
    CONSTRAINT chk_poin_jumlah     CHECK (jumlah_poin != 0)
);

CREATE INDEX IF NOT EXISTS ix_poin_member   ON tb_poin_log (id_member, tanggal);
CREATE INDEX IF NOT EXISTS ix_poin_tanggal  ON tb_poin_log (tanggal);
