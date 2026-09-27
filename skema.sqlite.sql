-- ===================================================================
--  SKEMA DATABASE APLIKASI KASIR SMK NEGERI 4  (SQLite)
-- ===================================================================
--  Berkas ini adalah skema instalasi baru untuk aplikasi kasir.
--  Database.cs membacanya saat pertama kali dijalankan, sehingga
--  tidak perlu mysql.exe atau XAMPP sama sekali. Semua data
--  disimpan dalam satu berkas bernama kasir.db
--
--  Untuk memasang ulang dari nol secara manual:
--      sqlite3 kasir.db < skema.sqlite.sql
--
--  SELURUH BERKAS INI HANYA MENGANDUNG KARAKTER ASCII.
-- ===================================================================


-- -------------------------------------------------------------------
--  1. tb_user  (akun pemakai aplikasi)
-- -------------------------------------------------------------------
--  role dibatasi lewat CHECK constraint, bukan tipe ENUM, karena
--  SQLite tidak punya tipe ENUM. Daftar nilainya harus selalu sama
--  dengan SessionIsRoleValid() di Form1.cs.
--
--  is_active memakai INTEGER 0/1, bukan TINYINT(1). Pada MySQL,
--  TINYINT(1) dibaca driver sebagai System.Boolean sehingga
--  aritmetika seperti 1 - nilai_boolean gagal diam-diam. INTEGER
--  dibaca sebagai Int64, jadi jebakan tipe itu hilang sendiri.
CREATE TABLE IF NOT EXISTS tb_user
(
    id_user        INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    nama_lengkap   TEXT    NOT NULL,
    username       TEXT    NOT NULL,
    password       TEXT    NOT NULL,
    role           TEXT    NOT NULL DEFAULT 'Kasir',
    is_active      INTEGER NOT NULL DEFAULT 1
                    CHECK (is_active IN (0, 1)),
    dibuat_pada    TEXT    NOT NULL
                    DEFAULT (strftime('%Y-%m-%d %H:%M:%S', 'now', 'localtime')),

    CONSTRAINT uq_user_username UNIQUE (username),
    CONSTRAINT chk_user_role    CHECK (role IN ('Admin', 'Kasir'))
);

CREATE INDEX IF NOT EXISTS ix_user_aktif ON tb_user (is_active, role);


-- -------------------------------------------------------------------
--  1b. Catatan versi skema
-- -------------------------------------------------------------------
--  Tabel tb_skema_versi sengaja TIDAK dibuat di berkas ini. Tabel itu
--  dibuat oleh Database.PastikanAdaTabelVersi di kode C#, karena
--  database lama yang sudah jadi juga butuh tabel itu, dan database
--  lama tidak pernah melewati berkas ini lagi.
--
--  Menulisnya di dua tempat berarti definisi yang sama ada dua kali,
--  dan yang kedua bisa diam-diam berbeda dari yang pertama. Uji
--  "AG. Instalasi baru = hasil migrasi" membandingkan kedua jalur itu
--  karakter demi karakter, jadi perbedaan sekecil spasi akan ketahuan.
--
--  Berkas ini tetap bisa dipakai sendiri sebagai installer:
--      sqlite3 kasir.db < skema.sqlite.sql
--  Setelah itu aplikasi akan membuat tabel versi, menandai versi 1,
--  lalu menjalankan migrasi yang ada. Hasilnya sama persis.


-- -------------------------------------------------------------------
--  2. tb_barang  (master barang)
-- -------------------------------------------------------------------
--  Kolom uang memakai tipe NUMERIC, bukan DECIMAL(15,2) seperti di
--  MySQL, karena SQLite tidak punya desimal presisi-tetap. SQLite
--  menyimpan NUMERIC sebagai bilangan pecahan biner, sehingga
--  perkalian di dalam agregat bisa menghasilkan lebih dari dua desimal.
--  Angka di bawah hasil pengukuran langsung di SQLite 3.53.3:
--      1235.16 * 0.1        = 123,516
--      1235.16 * 0.2        = 247,032
--      1235.16 * 0.3        = 370,548
--      SUM(harga * qty)     = 741,096    <-- tiga desimal
--      ROUND(SUM(...), 2)   = 741,10
--  Qty pecahan bukan kasus tepi di aplikasi ini karena ada penjualan
--  barang timbangan. Karena itu setiap query agregat uang WAJIB
--  dibungkus ROUND.
--      benar:  SELECT ROUND(SUM(total_bayar), 2) FROM ...
--      salah:  SELECT SUM(total_bayar) FROM ...
--
--  Dua desimal dijaga oleh syarat ROUND(x,2) = x pada tiap kolom
--  uang, meniru jaminan DECIMAL(15,2) yang dulu ada di MySQL. Syarat
--  ini juga menolak produk yang belum dibulatkan, jadi aplikasi wajib
--  membulatkan subtotal per item sebelum menyimpannya. Yang mengerjakan
--  itu adalah FormKasir.BulatkanSubtotal.
--
--  is_active hanya boleh 0 atau 1 supaya soft delete tetap tegakan.
CREATE TABLE IF NOT EXISTS tb_barang
(
    kode_barcode    TEXT    NOT NULL PRIMARY KEY,
    nama_barang     TEXT    NOT NULL,
    satuan          TEXT    NOT NULL DEFAULT 'pcs',
    harga_beli      NUMERIC NOT NULL DEFAULT 0,
    harga_jual      NUMERIC NOT NULL DEFAULT 0,
    stok            NUMERIC NOT NULL DEFAULT 0,
    minimal_grosir  NUMERIC NOT NULL DEFAULT 0,
    harga_grosir    NUMERIC NOT NULL DEFAULT 0,
    is_active       INTEGER NOT NULL DEFAULT 1
                     CHECK (is_active IN (0, 1)),
    dibuat_pada     TEXT    NOT NULL
                     DEFAULT (strftime('%Y-%m-%d %H:%M:%S', 'now', 'localtime')),
    diperbarui_pada TEXT    NOT NULL
                     DEFAULT (strftime('%Y-%m-%d %H:%M:%S', 'now', 'localtime')),

    CONSTRAINT chk_barang_harga_beli CHECK (harga_beli >= 0
        AND ROUND(harga_beli, 2) = harga_beli),
    CONSTRAINT chk_barang_harga_jual CHECK (harga_jual > 0
        AND ROUND(harga_jual, 2) = harga_jual),
    CONSTRAINT chk_barang_stok       CHECK (stok >= 0),
    CONSTRAINT chk_barang_grosir     CHECK (minimal_grosir >= 0
        AND harga_grosir >= 0)
);

CREATE INDEX IF NOT EXISTS ix_barang_nama  ON tb_barang (nama_barang);
CREATE INDEX IF NOT EXISTS ix_barang_aktif ON tb_barang (is_active, nama_barang);


-- -------------------------------------------------------------------
--  2b. Trigger cap waktu perubahan barang
-- -------------------------------------------------------------------
--  SQLITE tidak mendukung ON UPDATE CURRENT_TIMESTAMP, jadi kolom
--  diperbarui_pada diisi lewat trigger AFTER.
--
--  Dua jebakan sudah dicoba dan ditengok uji di sini. Keduanya
--  sengaja dicatat supaya tidak terulang:
--
--  1. Bentuk  NEW.diperbarui_pada = nilai  TIDAK berlaku di SQLite.
--     Itu sintaks PostgreSQL dan Oracle. SQLite menolaknya dengan
--     pesan "near NEW: syntax error". Cara resmi SQLite untuk
--     mengubah nilai NEW di trigger BEFORE adalah menulis UPDATE
--     atau INSERT bersarang, bukan assignment langsung.
--
--  2. Syarat  WHEN OLD.diperbarui_pada IS NOT NEW.diperbarui_pada
--     pada trigger AFTER selalu bernilai salah. Saat AFTER
--     berjalan, kolom diperbarui_pada masih menyimpan nilai lama
--     sehingga syarat itu tidak pernah terpenuhi, dan badannya
--     tidak pernah dijalankan. Trigger terlihat terpasang, lalu
--     diam-diam tidak mengubah apa pun. Karena itu trigger ini
--     sengaja TIDAK memakai WHEN.
--
--  Soal pengulangan: trigger ini menjalankan UPDATE pada tabel
--  yang sama, jadi ia memanggil dirinya sendiri kalau PRAGMA
--  recursive_triggers diaktifkan. Selama recursive_triggers
--  dimatikan hal itu tidak terjadi, dan Database.cs memaksa
--  pragma itu mati pada setiap koneksi. Jangan mengubah pragma
--  itu tanpa membaca trigger ini lebih dulu.
--
--  Resolusi waktu hanya satu detik, sama seperti yang sudah cukup
--  untuk jejak audit. Detik tidak pecah dan tidak memakai
--  karakter titik, sehingga perbandingan BETWEEN tanggal tetap
--  berupa string yang bisa diurutkan secara leksikografis.
CREATE TRIGGER IF NOT EXISTS trg_barang_updated
AFTER UPDATE ON tb_barang
FOR EACH ROW
BEGIN
    UPDATE tb_barang
       SET diperbarui_pada = strftime('%Y-%m-%d %H:%M:%S', 'now', 'localtime')
     WHERE kode_barcode = NEW.kode_barcode;
END;


-- -------------------------------------------------------------------
--  3. tb_transaksi  (header nota)
-- -------------------------------------------------------------------
--  no_nota tetap UNIQUE supaya satu nota hanya bisa muncul sekali.
--  id_transaksi memakai AUTOINCREMENT, bukan INTEGER PRIMARY KEY
--  polos, karena aplikasi memakai id ini sebagai jejak audit:
--  nomor urut tidak boleh pernah dipakai ulang walaupun ada baris
--  yang dihapus.
--
--  ON UPDATE CASCADE tidak didukung SQLite dan tidak dicantumkan.
--  Untuk id_user yang tidak pernah berubah, itu pilihan yang benar.
CREATE TABLE IF NOT EXISTS tb_transaksi
(
    id_transaksi   INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    no_nota        TEXT    NOT NULL,
    tanggal        TEXT    NOT NULL
                    DEFAULT (strftime('%Y-%m-%d %H:%M:%S', 'now', 'localtime')),
    id_user        INTEGER NOT NULL,
    total_bayar    NUMERIC NOT NULL DEFAULT 0,
    diskon_total   NUMERIC NOT NULL DEFAULT 0,
    uang_diterima  NUMERIC NOT NULL DEFAULT 0,
    kembalian      NUMERIC NOT NULL DEFAULT 0,
    catatan        TEXT,

    CONSTRAINT fk_transaksi_user        FOREIGN KEY (id_user)
        REFERENCES tb_user (id_user) ON DELETE RESTRICT,
    CONSTRAINT uq_transaksi_no_nota     UNIQUE (no_nota),
    CONSTRAINT chk_transaksi_total      CHECK (total_bayar >= 0
        AND ROUND(total_bayar, 2) = total_bayar),
    CONSTRAINT chk_transaksi_diterima   CHECK (uang_diterima >= 0
        AND ROUND(uang_diterima, 2) = uang_diterima),
    CONSTRAINT chk_transaksi_kembali    CHECK (kembalian >= 0
        AND ROUND(kembalian, 2) = kembalian),
    CONSTRAINT chk_transaksi_diskon     CHECK (diskon_total >= 0
        AND ROUND(diskon_total, 2) = diskon_total)
);

CREATE INDEX IF NOT EXISTS ix_transaksi_tanggal ON tb_transaksi (tanggal);
CREATE INDEX IF NOT EXISTS ix_transaksi_user    ON tb_transaksi (id_user, tanggal);


-- -------------------------------------------------------------------
--  4. tb_detail_transaksi  (baris item tiap nota)
-- -------------------------------------------------------------------
--  nama_barang dan harga_satuan adalah SNAPSHOT nilai saat transaksi
--  dibuat, bukan referensi ke tb_barang. Kalau harga atau nama
--  barang berubah di master, nota lama tidak boleh ikut berubah.
--
--  ON DELETE CASCADE dari id_transaksi dipertahankan karena inilah
--  yang membuat pembatalan nota ikut menghapus detailnya. ON UPDATE
--  CASCADE tidak didukung SQLite.
CREATE TABLE IF NOT EXISTS tb_detail_transaksi
(
    id_detail      INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    id_transaksi   INTEGER NOT NULL,
    no_nota        TEXT    NOT NULL,
    kode_barcode   TEXT    NOT NULL,
    nama_barang    TEXT    NOT NULL,
    qty            NUMERIC NOT NULL,
    harga_satuan   NUMERIC NOT NULL,
    subtotal       NUMERIC NOT NULL DEFAULT 0,
    diskon_item    NUMERIC NOT NULL DEFAULT 0,

    CONSTRAINT fk_detail_transaksi    FOREIGN KEY (id_transaksi)
        REFERENCES tb_transaksi (id_transaksi) ON DELETE CASCADE,
    CONSTRAINT fk_detail_barang       FOREIGN KEY (kode_barcode)
        REFERENCES tb_barang (kode_barcode) ON DELETE RESTRICT,
    CONSTRAINT chk_detail_qty         CHECK (qty > 0),
    CONSTRAINT chk_detail_harga_satuan CHECK (harga_satuan >= 0
        AND ROUND(harga_satuan, 2) = harga_satuan),
    CONSTRAINT chk_detail_subtotal     CHECK (subtotal >= 0
        AND ROUND(subtotal, 2) = subtotal),
    CONSTRAINT chk_detail_diskon       CHECK (diskon_item >= 0
        AND ROUND(diskon_item, 2) = diskon_item)
);

CREATE INDEX IF NOT EXISTS ix_detail_barang ON tb_detail_transaksi (kode_barcode);
CREATE INDEX IF NOT EXISTS ix_detail_nota   ON tb_detail_transaksi (no_nota);


-- -------------------------------------------------------------------
--  5. tb_mutasi_stok  (ledger audit perubahan stok)
-- -------------------------------------------------------------------
--  Setiap perubahan stok harus menambah satu baris di sini, baik itu
--  penjualan, pembelian, penyesuaian opname, atau pembatalan nota.
--  qty positif berarti stok masuk, negatif berarti stok keluar.
--
--  tipe memakai TEXT plus CHECK, bukan ENUM, karena SQLite tidak
--  punya tipe ENUM. Daftar nilainya harus sama persis dengan enum
--  MySQL yang lama.
--
--  Perhatikan: kolomnya bernama ref_no_nota, bukan no_nota.
CREATE TABLE IF NOT EXISTS tb_mutasi_stok
(
    id_mutasi      INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    tanggal        TEXT    NOT NULL
                    DEFAULT (strftime('%Y-%m-%d %H:%M:%S', 'now', 'localtime')),
    kode_barcode   TEXT    NOT NULL,
    tipe           TEXT    NOT NULL,
    qty            NUMERIC NOT NULL,
    stok_akhir     NUMERIC NOT NULL,
    keterangan     TEXT,
    id_user        INTEGER,
    ref_no_nota    TEXT,

    CONSTRAINT fk_mutasi_barang   FOREIGN KEY (kode_barcode)
        REFERENCES tb_barang (kode_barcode) ON DELETE RESTRICT,
    CONSTRAINT fk_mutasi_user     FOREIGN KEY (id_user)
        REFERENCES tb_user (id_user) ON DELETE SET NULL,
    CONSTRAINT chk_mutasi_tipe    CHECK (tipe IN ('MASUK', 'KELUAR', 'PENYESUAIAN'))
);

CREATE INDEX IF NOT EXISTS ix_mutasi_barang  ON tb_mutasi_stok (kode_barcode, tanggal);
CREATE INDEX IF NOT EXISTS ix_mutasi_tanggal ON tb_mutasi_stok (tanggal);
CREATE INDEX IF NOT EXISTS ix_mutasi_tipe    ON tb_mutasi_stok (tipe);


-- -------------------------------------------------------------------
--  6. View laporan
-- -------------------------------------------------------------------
--  Kolom nilai_persediaan dibungkus ROUND karena perkalian stok kali
--  harga bisa menghasilkan galat desimal panjang.
DROP VIEW IF EXISTS v_laporan_penjualan;
CREATE VIEW v_laporan_penjualan AS
SELECT
    t.no_nota        AS no_nota,
    t.tanggal        AS tanggal,
    u.username       AS username,
    u.nama_lengkap   AS nama_lengkap,
    d.kode_barcode   AS kode_barcode,
    d.nama_barang    AS nama_barang,
    d.qty            AS qty,
    d.harga_satuan   AS harga_satuan,
    d.diskon_item    AS diskon_item,
    d.subtotal       AS subtotal
FROM tb_transaksi t
JOIN tb_user u
  ON u.id_user = t.id_user
JOIN tb_detail_transaksi d
  ON d.id_transaksi = t.id_transaksi;

DROP VIEW IF EXISTS v_stok_gudang;
CREATE VIEW v_stok_gudang AS
SELECT
    b.kode_barcode   AS kode_barcode,
    b.nama_barang    AS nama_barang,
    b.satuan         AS satuan,
    b.harga_beli     AS harga_beli,
    b.harga_jual     AS harga_jual,
    b.stok           AS stok,
    ROUND(b.stok * b.harga_beli, 2) AS nilai_persediaan,
    b.is_active      AS is_active
FROM tb_barang b;


-- -------------------------------------------------------------------
--  7. Data awal
-- -------------------------------------------------------------------
--  Berkas ini sengaja TIDAK memuat INSERT untuk akun admin.
--  Database.cs membuat akun tersebut saat pertama kali dijalankan,
--  memakai salt acak lewat PasswordHasher. Hash yang ditulis mati
--  di dalam berkas SQL justru berbahaya: siapa pun yang membaca
--  berkas ini bisa mengambil hash admin lalu mencobanya offline.
--
--  Kalau berkas ini dipasang manual dengan perintah
--      sqlite3 kasir.db < skema.sqlite.sql
--  tabel akan terbentuk tetapi belum ada user. Jalankan aplikasi
--  sekali, Database.cs akan mengisi akun admin otomatis.
--
--  GANTI password admin bawaan sebelum dipakai di kasir sungguhan.
-- -------------------------------------------------------------------
