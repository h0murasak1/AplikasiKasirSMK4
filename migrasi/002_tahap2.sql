-- ===================================================================
--  MIGRASI 002 - TAHAP 2
--  Dijalankan oleh Database.cs pada database yang sudah versi 1.
--
--  SELURUH BERKAS INI HANYA MENGANDUNG KARAKTER ASCII.
--
--  Tanpa file ini sama sekali: database versi 1 punya 5 tabel, 10
--  index, 2 view, 1 trigger, 5 foreign key. Tidak ada perubahan pada
--  objek-objek itu. Yang ditambahkan semuanya baru.
--
--  Kolom baru pada tabel yang sudah ada memakai ALTER TABLE ADD
--  COLUMN, bukan dibangun ulang. Alasannya, membangun ulang tabel di
--  SQLite berarti membuat tabel sementara lalu memindahkan data, dan
--  SQLite menulis ulang rujukan foreign key di tabel lain secara
--  otomatis begitu tabel lama di-rename. Itu rawan merusak data.
--  Menambah kolom tidak menyentuh data yang sudah ada sama sekali.
--
--  Konsekuensi yang sengaja diterima: SQLite tidak bisa menambah
--  CHECK constraint ke tabel yang sudah ada. Jadi kolom baru di sini
--  tidak punya CHECK, dan aturan nilainya ditegakkan di sisi aplikasi.
--  Instalasi baru dan instalasi yang dimigrasi punya struktur yang
--  sama persis, jadi tidak ada dua bentuk skema yang berbeda.
--
--  Kolom yang jadi foreign key tetap boleh NULL. SQLite hanya
--  memeriksa foreign key untuk baris baru, dan baris lama tidak
--  boleh ikut diperiksa. NULL dipakai sebagai arti "belum diisi".
-- ===================================================================


-- -------------------------------------------------------------------
--  1. tb_profil_toko
-- -------------------------------------------------------------------
--  Id_toko dikunci ke angka 1 lewat CHECK, jadi tabel ini maksimal
--  berisi satu baris. Bentuknya satu baris, bukan pasangan
--  key-value, supaya semua kolomnya terlihat jelas di skema.
--
--  PPN ikut disimpan di sini karena persen PPN adalah keputusan toko,
--  bukan keputusan aplikasi. ppn_persen dibatasi 0 sampai 100, supaya
--  tidak mungkin berisi 11000 yang salah ketik.
--
--  id_toko tetap 1 selamanya. Kalau nanti mendukung lebih dari satu
--  toko, tabel ini dipecah dan kolom id_toko ditambahkan ke tabel
--  lain saat itu juga, bukan sekarang.
--
--  chk_profil_ppn melarang PPN hidup dengan persen nol. Bentuknya
--  "ppn_aktif = 0 ATAU ppn_persen > 0": kalau PPN mati, persennya
--  bebas; kalau PPN hidup, persennya wajib lebih dari nol. Menulisnya
--  terbalik akan membiarkan PPN hidup tanpa persen, dan setiap nota
--  jadi berisi nol PPN tanpa ada yang sadar.
CREATE TABLE IF NOT EXISTS tb_profil_toko
(
    id_toko         INTEGER NOT NULL PRIMARY KEY
                    CHECK (id_toko = 1),
    nama_toko       TEXT    NOT NULL,
    nama_pemilik    TEXT,
    alamat          TEXT,
    telepon         TEXT,
    email           TEXT,
    npwp            TEXT,
    logo_path       TEXT,
    catatan_struk   TEXT,
    ppn_aktif       INTEGER NOT NULL DEFAULT 0
                    CHECK (ppn_aktif IN (0, 1)),
    ppn_persen      NUMERIC NOT NULL DEFAULT 0
                    CHECK (ppn_persen >= 0 AND ppn_persen <= 100
                        AND ROUND(ppn_persen, 2) = ppn_persen),
    diperbarui_pada TEXT    NOT NULL
                    DEFAULT (strftime('%Y-%m-%d %H:%M:%S', 'now', 'localtime')),

    CONSTRAINT chk_profil_ppn CHECK (ppn_aktif = 0 OR ppn_persen > 0)
);


-- -------------------------------------------------------------------
--  2. tb_jenis_barang  (kategori barang)
-- -------------------------------------------------------------------
--  Empat tabel master berikutnya sengaja dibuat dengan bentuk kolom
--  yang sama persis: id, nama, keterangan, is_active, dan dua kolom
--  waktu. Bentuk yang sama itu yang membuat satu FormMaster generik
--  bisa melayani keempatnya, tanpa menulis ulang empat form.
CREATE TABLE IF NOT EXISTS tb_jenis_barang
(
    id_jenis        INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    nama_jenis      TEXT    NOT NULL,
    keterangan      TEXT,
    is_active       INTEGER NOT NULL DEFAULT 1
                    CHECK (is_active IN (0, 1)),
    dibuat_pada     TEXT    NOT NULL
                    DEFAULT (strftime('%Y-%m-%d %H:%M:%S', 'now', 'localtime')),
    diperbarui_pada TEXT    NOT NULL
                    DEFAULT (strftime('%Y-%m-%d %H:%M:%S', 'now', 'localtime')),

    CONSTRAINT uq_jenis_nama UNIQUE (nama_jenis)
);

CREATE INDEX IF NOT EXISTS ix_jenis_aktif ON tb_jenis_barang (is_active, nama_jenis);


-- -------------------------------------------------------------------
--  3. tb_merek
-- -------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS tb_merek
(
    id_merek        INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    nama_merek      TEXT    NOT NULL,
    keterangan      TEXT,
    is_active       INTEGER NOT NULL DEFAULT 1
                    CHECK (is_active IN (0, 1)),
    dibuat_pada     TEXT    NOT NULL
                    DEFAULT (strftime('%Y-%m-%d %H:%M:%S', 'now', 'localtime')),
    diperbarui_pada TEXT    NOT NULL
                    DEFAULT (strftime('%Y-%m-%d %H:%M:%S', 'now', 'localtime')),

    CONSTRAINT uq_merek_nama UNIQUE (nama_merek)
);

CREATE INDEX IF NOT EXISTS ix_merek_aktif ON tb_merek (is_active, nama_merek);
-- -------------------------------------------------------------------
--  4. tb_supplier
-- -------------------------------------------------------------------
--  Bentuk kolom sama seperti dua tabel di atas, ditambah tiga kolom
--  kontak. Kolom tambahan ini diabaikan FormMaster kalau metadata-nya
--  tidak mendaftarkannya, jadi bentuk inti tetap sama.
CREATE TABLE IF NOT EXISTS tb_supplier
(
    id_supplier     INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    nama_supplier   TEXT    NOT NULL,
    keterangan      TEXT,
    telepon         TEXT,
    alamat          TEXT,
    nama_kontak     TEXT,
    is_active       INTEGER NOT NULL DEFAULT 1
                    CHECK (is_active IN (0, 1)),
    dibuat_pada     TEXT    NOT NULL
                    DEFAULT (strftime('%Y-%m-%d %H:%M:%S', 'now', 'localtime')),
    diperbarui_pada TEXT    NOT NULL
                    DEFAULT (strftime('%Y-%m-%d %H:%M:%S', 'now', 'localtime')),

    CONSTRAINT uq_supplier_nama UNIQUE (nama_supplier)
);

CREATE INDEX IF NOT EXISTS ix_supplier_aktif ON tb_supplier (is_active, nama_supplier);


-- -------------------------------------------------------------------
--  5. tb_sales
-- -------------------------------------------------------------------
--  komisi_persen adalah persen dari nilai nota, bukan per item.
--  Angka 0 berarti sales ini belum ditunjuk menerima komisi.
--  Nilainya dibatasi 0 sampai 100 supaya tidak mungkin berisi angka
--  salah ketik jadi ribuan persen.
CREATE TABLE IF NOT EXISTS tb_sales
(
    id_sales        INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    nama_sales      TEXT    NOT NULL,
    kode_sales      TEXT,
    keterangan      TEXT,
    telepon         TEXT,
    komisi_persen   NUMERIC NOT NULL DEFAULT 0
                    CHECK (komisi_persen >= 0 AND komisi_persen <= 100
                        AND ROUND(komisi_persen, 2) = komisi_persen),
    is_active       INTEGER NOT NULL DEFAULT 1
                    CHECK (is_active IN (0, 1)),
    dibuat_pada     TEXT    NOT NULL
                    DEFAULT (strftime('%Y-%m-%d %H:%M:%S', 'now', 'localtime')),
    diperbarui_pada TEXT    NOT NULL
                    DEFAULT (strftime('%Y-%m-%d %H:%M:%S', 'now', 'localtime')),

    CONSTRAINT uq_sales_nama UNIQUE (nama_sales)
);

CREATE INDEX IF NOT EXISTS ix_sales_aktif ON tb_sales (is_active, nama_sales);


-- -------------------------------------------------------------------
--  6. Kolom penunjuk pada tb_barang
-- -------------------------------------------------------------------
--  Ditambah sebagai kolom yang boleh NULL. Barang lama tidak punya
--  jenis, merek, maupun supplier, jadi NULL berarti "belum diisi" dan
--  FormBarang menampilkan "Tanpa jenis" sebagai pilihannya.
ALTER TABLE tb_barang ADD COLUMN id_jenis    INTEGER REFERENCES tb_jenis_barang (id_jenis);
ALTER TABLE tb_barang ADD COLUMN id_merek    INTEGER REFERENCES tb_merek (id_merek);
ALTER TABLE tb_barang ADD COLUMN id_supplier INTEGER REFERENCES tb_supplier (id_supplier);

CREATE INDEX IF NOT EXISTS ix_barang_jenis    ON tb_barang (id_jenis);
CREATE INDEX IF NOT EXISTS ix_barang_merek    ON tb_barang (id_merek);
CREATE INDEX IF NOT EXISTS ix_barang_supplier ON tb_barang (id_supplier);
-- -------------------------------------------------------------------
--  7. tb_member
-- -------------------------------------------------------------------
--  kode_member dibuat oleh aplikasi dengan format MBR0001, MBR0002,
--  dan seterusnya. UNIQUE di kolom itu yang menjaga agar kode yang
--  sama tidak dipakai dua member.
--
--  tier hanya untuk tampilan, bukan untuk aturan harga. Diskon yang
--  benar-benar dipakai saat transaksi diambil dari kolom
--  diskon_persen, bukan dari tier. Jadi mengubah tier tidak pernah
--  mengubah harga nota lama, dan tidak pernah diam-diam mengubah
--  harga tanpa sengaja.
--
--  poin adalah saldo berjalan. Setiap perubahan dicatat di
--  tb_poin_member, jadi kolom ini selalu bisa dihitung ulang dari
--  riwayat bila terjadi ketidakcocokan.
CREATE TABLE IF NOT EXISTS tb_member
(
    id_member       INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    kode_member     TEXT    NOT NULL,
    nama_member     TEXT    NOT NULL,
    telepon         TEXT,
    alamat          TEXT,
    email           TEXT,
    tier            TEXT    NOT NULL DEFAULT 'Bronze'
                    CHECK (tier IN ('Bronze', 'Silver', 'Gold', 'Platinum')),
    poin            INTEGER NOT NULL DEFAULT 0
                    CHECK (poin >= 0),
    diskon_persen   NUMERIC NOT NULL DEFAULT 0
                    CHECK (diskon_persen >= 0 AND diskon_persen <= 100
                        AND ROUND(diskon_persen, 2) = diskon_persen),
    is_active       INTEGER NOT NULL DEFAULT 1
                    CHECK (is_active IN (0, 1)),
    dibuat_pada     TEXT    NOT NULL
                    DEFAULT (strftime('%Y-%m-%d %H:%M:%S', 'now', 'localtime')),
    diperbarui_pada TEXT    NOT NULL
                    DEFAULT (strftime('%Y-%m-%d %H:%M:%S', 'now', 'localtime')),

    CONSTRAINT uq_member_kode UNIQUE (kode_member)
);

CREATE INDEX IF NOT EXISTS ix_member_nama  ON tb_member (nama_member);
CREATE INDEX IF NOT EXISTS ix_member_aktif ON tb_member (is_active, nama_member);


-- -------------------------------------------------------------------
--  8. tb_poin_member  (ledger poin)
-- -------------------------------------------------------------------
--  Setiap perubahan saldo poin punya satu baris di sini. Baris
--  positif untuk TAMBAH, negatif untuk PAKAI. Saldo berjalan dihitung
--  lewat SUM dari riwayat, bukan lewat kolom sisa yang disimpan, jadi
--  saldo tidak mungkin melenceng karena pembulatan.
--
--  Baris di sini tidak pernah dihapus. Koreksi dibuat sebagai baris
--  baru bertipe KOREKSI, bukan dengan menghapus atau mengubah baris
--  lama. Itulah yang membuat riwayat member bisa dipertanggungjawabkan.
--
--  chk_poin_nol melarang baris bernilai nol, karena baris seperti itu
--  tidak mengubah apa pun tapi tetap memenuhi syarat kolom poin.
CREATE TABLE IF NOT EXISTS tb_poin_member
(
    id_poin         INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    tanggal         TEXT    NOT NULL
                    DEFAULT (strftime('%Y-%m-%d %H:%M:%S', 'now', 'localtime')),
    id_member       INTEGER NOT NULL,
    tipe            TEXT    NOT NULL
                    CHECK (tipe IN ('TAMBAH', 'PAKAI', 'KOREKSI')),
    poin            INTEGER NOT NULL,
    no_nota         TEXT,
    keterangan      TEXT,
    id_user         INTEGER,

    CONSTRAINT fk_poin_member FOREIGN KEY (id_member)
        REFERENCES tb_member (id_member) ON DELETE CASCADE,
    CONSTRAINT fk_poin_user   FOREIGN KEY (id_user)
        REFERENCES tb_user (id_user) ON DELETE SET NULL,
    CONSTRAINT chk_poin_nol   CHECK (poin <> 0)
);

CREATE INDEX IF NOT EXISTS ix_poin_member ON tb_poin_member (id_member, tanggal);
CREATE INDEX IF NOT EXISTS ix_poin_nota   ON tb_poin_member (no_nota);
-- -------------------------------------------------------------------
--  9. tb_metode_bayar
-- -------------------------------------------------------------------
--  jenis membedakan uang yang diterima langsung, uang nontunai, dan
--  uang yang dicatat sebagai piutang. Metode bertipe Tempo berubah
--  jadi piutang, jadi besarannya tidak boleh nol.
--
--  Kolom urutan menentukan urutan muncul di kasir. Admin bisa
--  menaruhnya supaya metode yang paling sering dipakai paling atas.
CREATE TABLE IF NOT EXISTS tb_metode_bayar
(
    id_metode       INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    nama_metode     TEXT    NOT NULL,
    jenis           TEXT    NOT NULL
                    CHECK (jenis IN ('TUNAI', 'NON_TUNAI', 'TEMPO')),
    keterangan      TEXT,
    urutan          INTEGER NOT NULL DEFAULT 0,
    is_aktif        INTEGER NOT NULL DEFAULT 1
                    CHECK (is_aktif IN (0, 1)),

    CONSTRAINT uq_metode_nama UNIQUE (nama_metode)
);

CREATE INDEX IF NOT EXISTS ix_metode_aktif ON tb_metode_bayar (is_aktif, urutan);


-- -------------------------------------------------------------------
--  10. Kolom tambahan pada tb_transaksi
-- -------------------------------------------------------------------
--  subtotal, nilai_ppn, dan diskon_total dipisah supaya laporan laba
--  tidak perlu menebak PPN dari total. total_bayar tetap isi yang
--  benar-benar ditagih ke pelanggan:
--
--      total_bayar = subtotal - diskon_total + nilai_ppn
--
--  is_void menandai nota yang dibatalkan. Void tidak menghapus data
--  sama sekali, karena nota yang sudah dicetak ke kertas tidak boleh
--  hilang. Semua angka pada nota yang void tetap utuh, hanya tidak
--  dihitung di laporan penjualan dan tidak mengurangi stok.
--
--  Kolom ini tidak punya CHECK karena SQLite tidak bisa menambah
--  CHECK ke tabel yang sudah ada. Aturan 0 atau 1-nya ditegakkan di
--  sisi aplikasi, dan nilainya tidak pernah datang dari input
--  pengguna secara langsung.
ALTER TABLE tb_transaksi ADD COLUMN id_member    INTEGER REFERENCES tb_member (id_member);
ALTER TABLE tb_transaksi ADD COLUMN id_metode    INTEGER REFERENCES tb_metode_bayar (id_metode);
ALTER TABLE tb_transaksi ADD COLUMN id_sales     INTEGER REFERENCES tb_sales (id_sales);
ALTER TABLE tb_transaksi ADD COLUMN subtotal     NUMERIC NOT NULL DEFAULT 0;
ALTER TABLE tb_transaksi ADD COLUMN nilai_ppn    NUMERIC NOT NULL DEFAULT 0;
ALTER TABLE tb_transaksi ADD COLUMN ppn_persen   NUMERIC NOT NULL DEFAULT 0;
ALTER TABLE tb_transaksi ADD COLUMN poin_dipakai INTEGER NOT NULL DEFAULT 0;
ALTER TABLE tb_transaksi ADD COLUMN nilai_poin   NUMERIC NOT NULL DEFAULT 0;
ALTER TABLE tb_transaksi ADD COLUMN is_void      INTEGER NOT NULL DEFAULT 0;
ALTER TABLE tb_transaksi ADD COLUMN void_alasan  TEXT;
ALTER TABLE tb_transaksi ADD COLUMN void_pada    TEXT;
ALTER TABLE tb_transaksi ADD COLUMN void_by      INTEGER REFERENCES tb_user (id_user);

CREATE INDEX IF NOT EXISTS ix_transaksi_member ON tb_transaksi (id_member, tanggal);
CREATE INDEX IF NOT EXISTS ix_transaksi_void   ON tb_transaksi (is_void, tanggal);
CREATE INDEX IF NOT EXISTS ix_transaksi_sales  ON tb_transaksi (id_sales, tanggal);


-- -------------------------------------------------------------------
--  11. Kolom tambahan pada tb_detail_transaksi
-- -------------------------------------------------------------------
--  harga_beli_satuan adalah snapshot, sama seperti harga_satuan.
--  Tanpa kolom ini laporan laba tidak bisa dihitung, karena harga
--  beli di master bisa berubah kapan saja, sementara nota lama
--  sudah terlanjur dibayar dengan harga beli yang berbeda.
--
--  tipe_harga mencatat harga mana yang dipakai, ECR atau GROSIR.
--  Ini bukan tampilan: laporan bisa menghitung berapa penjualan yang
--  terjadi di harga grosir tanpa perlu menebak dari angkanya.
ALTER TABLE tb_detail_transaksi ADD COLUMN id_sales          INTEGER REFERENCES tb_sales (id_sales);
ALTER TABLE tb_detail_transaksi ADD COLUMN harga_beli_satuan NUMERIC NOT NULL DEFAULT 0;
ALTER TABLE tb_detail_transaksi ADD COLUMN tipe_harga        TEXT    NOT NULL DEFAULT 'ECER';
-- -------------------------------------------------------------------
--  12. tb_komisi
-- -------------------------------------------------------------------
--  Satu baris per nota per sales. Persen ikut disimpan karena
--  kalau persen sales diubah di master, komisi nota lama tidak boleh
--  ikut berubah. Angka yang tersimpan itulah yang dibayar.
--
--  sudah_dibayar terpisah dari tanggal pembayaran, supaya daftar
--  komisi yang belum dibayar bisa difilter tanpa perlu memeriksa
--  apakah tanggalnya kosong.
CREATE TABLE IF NOT EXISTS tb_komisi
(
    id_komisi       INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    no_nota         TEXT    NOT NULL,
    id_transaksi    INTEGER NOT NULL,
    id_sales        INTEGER NOT NULL,
    tanggal         TEXT    NOT NULL
                    DEFAULT (strftime('%Y-%m-%d %H:%M:%S', 'now', 'localtime')),
    nilai_nota      NUMERIC NOT NULL,
    persen          NUMERIC NOT NULL
                    CHECK (persen >= 0 AND persen <= 100),
    jumlah_komisi   NUMERIC NOT NULL
                    CHECK (ROUND(jumlah_komisi, 2) = jumlah_komisi),
    sudah_dibayar   INTEGER NOT NULL DEFAULT 0,
    tanggal_dibayar TEXT,
    id_user         INTEGER,

    CONSTRAINT fk_komisi_transaksi FOREIGN KEY (id_transaksi)
        REFERENCES tb_transaksi (id_transaksi) ON DELETE CASCADE,
    CONSTRAINT fk_komisi_sales     FOREIGN KEY (id_sales)
        REFERENCES tb_sales (id_sales) ON DELETE RESTRICT,
    CONSTRAINT fk_komisi_user      FOREIGN KEY (id_user)
        REFERENCES tb_user (id_user) ON DELETE SET NULL,
    CONSTRAINT uq_komisi_nota      UNIQUE (no_nota, id_sales)
);

CREATE INDEX IF NOT EXISTS ix_komisi_sales  ON tb_komisi (id_sales, tanggal);
CREATE INDEX IF NOT EXISTS ix_komisi_bayar ON tb_komisi (sudah_dibayar, tanggal);


-- -------------------------------------------------------------------
--  13. tb_piutang
-- -------------------------------------------------------------------
--  Sisa piutang TIDAK disimpan sebagai kolom. Yang disimpan hanya
--  jumlah_piutang dan sudah_bayar, dan sisanya selalu dihitung saat
--  query dengan ROUND(jumlah_piutang - sudah_bayar, 2).
--
--  Menyimpan sisa sebagai kolom berarti setiap pembayaran sebagian
--  harus memperbarui dua angka sekaligus. Kalau salah satu lupa, isi
--  laporan piutang jadi tidak bisa dipercaya.
--
--  status juga tidak disimpan. Status berasal dari perbandingan
--  jumlah_piutang dengan sudah_bayar, jadi tidak mungkin lagi status
--  LUNAS sementara uangnya ternyata masih kurang.
--
--  ON DELETE CASCADE dari id_transaksi: kalau nota dihapus, piutang
--  yang dibuat dari nota itu ikut hilang. Pada aplikasi ini nota tidak
--  dihapus, yang ada baru penandaan void.
CREATE TABLE IF NOT EXISTS tb_piutang
(
    id_piutang      INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    no_nota         TEXT    NOT NULL,
    id_transaksi    INTEGER NOT NULL,
    id_member       INTEGER NOT NULL,
    tanggal         TEXT    NOT NULL
                    DEFAULT (strftime('%Y-%m-%d %H:%M:%S', 'now', 'localtime')),
    jatuh_tempo     TEXT,
    jumlah_piutang  NUMERIC NOT NULL
                    CHECK (jumlah_piutang > 0
                        AND ROUND(jumlah_piutang, 2) = jumlah_piutang),
    sudah_bayar     NUMERIC NOT NULL DEFAULT 0
                    CHECK (sudah_bayar >= 0
                        AND ROUND(sudah_bayar, 2) = sudah_bayar),
    keterangan      TEXT,

    CONSTRAINT fk_piutang_transaksi FOREIGN KEY (id_transaksi)
        REFERENCES tb_transaksi (id_transaksi) ON DELETE CASCADE,
    CONSTRAINT fk_piutang_member    FOREIGN KEY (id_member)
        REFERENCES tb_member (id_member) ON DELETE RESTRICT,
    CONSTRAINT uq_piutang_nota      UNIQUE (no_nota)
);

CREATE INDEX IF NOT EXISTS ix_piutang_member ON tb_piutang (id_member, tanggal);
CREATE INDEX IF NOT EXISTS ix_piutang_tempo  ON tb_piutang (jatuh_tempo);


-- -------------------------------------------------------------------
--  14. tb_piutang_bayar
-- -------------------------------------------------------------------
--  Satu piutang bisa dibayar beberapa kali. Setiap pembayaran punya
--  barisnya sendiri, jadi riwayat pembayaran tidak pernah hilang
--  ketika pembayaran berikutnya menimpa.
CREATE TABLE IF NOT EXISTS tb_piutang_bayar
(
    id_bayar        INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    id_piutang      INTEGER NOT NULL,
    tanggal         TEXT    NOT NULL
                    DEFAULT (strftime('%Y-%m-%d %H:%M:%S', 'now', 'localtime')),
    jumlah_bayar    NUMERIC NOT NULL
                    CHECK (jumlah_bayar > 0
                        AND ROUND(jumlah_bayar, 2) = jumlah_bayar),
    cara_bayar      TEXT,
    no_kuitansi     TEXT,
    keterangan      TEXT,
    id_user         INTEGER,

    CONSTRAINT fk_bayar_piutang FOREIGN KEY (id_piutang)
        REFERENCES tb_piutang (id_piutang) ON DELETE CASCADE,
    CONSTRAINT fk_bayar_user    FOREIGN KEY (id_user)
        REFERENCES tb_user (id_user) ON DELETE SET NULL
);

CREATE INDEX IF NOT EXISTS ix_bayar_piutang ON tb_piutang_bayar (id_piutang, tanggal);
CREATE INDEX IF NOT EXISTS ix_bayar_tanggal ON tb_piutang_bayar (tanggal);
-- -------------------------------------------------------------------
--  15. tb_opname  dan  tb_opname_detail
-- -------------------------------------------------------------------
--  Opname adalah penghitungan fisik. Kasir menghitung barang di
--  rak, lalu membandingkan dengan stok yang tercatat di sistem.
--
--  Stok hanya diubah setelah SELURUH baris opname disimpan, supaya
--  opname yang setengah jadi tidak pernah mengubah stok di tengah
--  jalan.
--
--  tb_opname menyimpan angka stok di sistem pada saat hitungan itu,
--  sehingga selisihnya masih bisa dihitung ulang walaupun stok sudah
--  berubah di meantime.
--
--  sudah_diperapkan menandai opname yang selisihnya sudah ditulis ke
--  tb_mutasi_stok. Yang belum diperapkan masih bisa diubah, karena
--  stok di sistem belum bergerak.
CREATE TABLE IF NOT EXISTS tb_opname
(
    id_opname        INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    nomor_opname     TEXT    NOT NULL,
    tanggal          TEXT    NOT NULL
                     DEFAULT (strftime('%Y-%m-%d %H:%M:%S', 'now', 'localtime')),
    id_user          INTEGER,
    catatan          TEXT,
    jumlah_item      INTEGER NOT NULL DEFAULT 0,
    selisih_nilai    NUMERIC NOT NULL DEFAULT 0,
    sudah_diperapkan INTEGER NOT NULL DEFAULT 0
                     CHECK (sudah_diperapkan IN (0, 1)),

    CONSTRAINT uq_opname_nomor UNIQUE (nomor_opname),
    CONSTRAINT fk_opname_user   FOREIGN KEY (id_user)
        REFERENCES tb_user (id_user) ON DELETE SET NULL
);

CREATE INDEX IF NOT EXISTS ix_opname_tanggal ON tb_opname (tanggal);

CREATE TABLE IF NOT EXISTS tb_opname_detail
(
    id_opname       INTEGER NOT NULL,
    kode_barcode    TEXT    NOT NULL,
    nama_barang     TEXT    NOT NULL,
    stok_sistem     NUMERIC NOT NULL,
    stok_fisik      NUMERIC NOT NULL
                    CHECK (stok_fisik >= 0),
    selisih         NUMERIC NOT NULL,
    harga_beli      NUMERIC NOT NULL,
    nilai_selisih   NUMERIC NOT NULL,
    catatan         TEXT,

    CONSTRAINT fk_opname_detail_opname FOREIGN KEY (id_opname)
        REFERENCES tb_opname (id_opname) ON DELETE CASCADE,
    CONSTRAINT fk_opname_detail_barang FOREIGN KEY (kode_barcode)
        REFERENCES tb_barang (kode_barcode) ON DELETE RESTRICT
);

CREATE INDEX IF NOT EXISTS ix_opname_detail_barang ON tb_opname_detail (kode_barcode);


-- -------------------------------------------------------------------
--  16. Data awal metode bayar
-- -------------------------------------------------------------------
--  Metode bawaan ditulis di sini, bukan oleh Database.cs, karena
--  isinya tidak mengandung rahasia apa pun. Password admin tetap
--  tidak ditulis di berkas SQL, itu urusan lain.
--
--  INSERT OR IGNORE dipakai supaya migrasi ini tetap aman dijalankan
--  berulang kali. Daftar metode bawaan: TUNAI, NON_TUNAI, TEMPO.
--
--  Metode Tempo memakai jenis TEMPO. Kasir memilihnya, lalu aplikasi
--  mencatat utang ke member sebagai gantinya meminta uang.
--
--  Catatan untuk pengelola: ubah daftar ini lewat menu Metode Bayar,
--  jangan mengedit berkas ini setelah rilis. Metode yang sudah dipakai
--  transaksi tidak boleh dihapus, hanya dinonaktifkan.
INSERT OR IGNORE INTO tb_metode_bayar (nama_metode, jenis, urutan) VALUES
    ('Tunai',           'TUNAI',    1),
    ('Debit / Kredit',  'NON_TUNAI', 2),
    ('QRIS',            'NON_TUNAI', 3),
    ('Transfer Bank',   'NON_TUNAI', 4),
    ('Tempo (Piutang)', 'TEMPO',    5);


-- -------------------------------------------------------------------
--  17. Data awal profil toko
-- -------------------------------------------------------------------
--  Satu baris, id_toko selalu 1. Kolom nama_toko dibiarkan kosong
--  supaya admin mengisinya sendiri lewat menu Profil Toko, dan tidak
--  ada nama toko yang dikarang oleh pembuat program.
--
--  ppn_aktif sengaja 0. PPN tidak boleh menyala tanpa keputusan
--  pemilik toko.
INSERT OR IGNORE INTO tb_profil_toko (id_toko, nama_toko, ppn_aktif, ppn_persen)
VALUES (1, '', 0, 0);