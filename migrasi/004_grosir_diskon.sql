-- ===================================================================
--  MIGRASI 004 - METODE BAYAR
--  Dijalankan oleh Database.cs pada database yang sudah versi 3.
-- ===================================================================

-- Pastikan seluruh metode pembayaran default terpasang jika belum ada
INSERT OR IGNORE INTO tb_metode_bayar (nama_metode, jenis, urutan, is_aktif) VALUES
    ('Tunai',           'TUNAI',     1, 1),
    ('Debit / Kredit',  'NON_TUNAI', 2, 1),
    ('QRIS',            'NON_TUNAI', 3, 1),
    ('Transfer Bank',   'NON_TUNAI', 4, 1),
    ('Tempo (Piutang)', 'TEMPO',     5, 1);
