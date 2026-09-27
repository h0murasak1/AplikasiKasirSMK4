# Aplikasi Kasir SMK Negeri 4

Aplikasi Point of Sale (POS) / mesin kasir berbasis **Windows Forms (C#)** untuk kebutuhan penjualan di **SMK Negeri 4**. Aplikasi ini menangani dua alur utama: **manajemen data barang (gudang)** oleh admin dan **transaksi penjualan dengan pemindaian barcode** oleh kasir.

---

## Daftar Isi

- [Tentang Aplikasi](#tentang-aplikasi)
- [Fitur](#fitur)
- [Teknologi](#teknologi)
- [Prasyarat Instalasi](#prasyarat-instalasi)
- [Setup Database](#setup-database)
- [Struktur Tabel](#struktur-tabel)
- [Konfigurasi Database](#konfigurasi-database)
- [Cara Menjalankan](#cara-menjalankan)
- [Akun Default](#akun-default)
- [Alur Penggunaan](#alur-penggunaan)
- [Struktur Proyek](#struktur-proyek)
- [Detail Perubahan Data](#detail-perubahan-data)
- [Tahap 1: Dari MySQL ke SQLite](#tahap-1-dari-mysql-ke-sqlite)
- [Cadangan dan Pemulihan](#cadangan-dan-pemulihan)
- [Ringkasan Perbaikan](#ringkasan-perbaikan)
- [Restrukturasi Database (zaman MySQL)](#restrukturasi-database-zaman-mysql)
- [Ide Pengembangan](#ide-pengembangan)
- [Lisensi](#lisensi)

---

## Tentang Aplikasi

Aplikasi kasir ini dibuat sebagai proyek untuk keperluan penjualan barang di lingkungan SMK Negeri 4. Dirancang dengan pendekatan sederhana: satu aplikasi desktop dengan seluruh datanya di dalam **satu berkas** `kasir.db` di samping executable, tanpa perlu server database, tanpa XAMPP, dan tanpa service yang harus dijalankan.

Seluruh folder aplikasi bisa disalin ke flashdisk, dijalankan dari sana, lalu dicabut setelah aplikasi ditutup. Membuat cadangan cukup menyalin satu berkas, bukan satu folder berisi puluhan ribu file.

Lima layar utama yang tersedia:

| Layar | Nama Form | Keterangan |
|---|---|---|
| Login | `Form1` | Autentikasi user dan pembagian hak akses |
| Dashboard Admin | `FormMenu` | Pintu masuk ke kelola barang, buka kasir, laporan, dan logout |
| Master Barang | `FormBarang` | CRUD data barang di gudang lengkap dengan pilihan satuan |
| Mesin Kasir | `FormKasir` | Scan barcode, keranjang belanja, pembayaran |
| Laporan Penjualan | `FormLaporan` | Filter periode, rekap nota, rincian item, ranking produk & ekspor CSV |

---

## Fitur

### 1. Login dan Hak Akses

- Validasi username dan password terhadap tabel `tb_user` di database.
- Sapaan otomatis menampilkan nama lengkap user.
- Pembagian role berbasis kolom `role`:
  - **Admin** diarahkan ke `FormMenu` (dashboard admin).
  - **Kasir** langsung diarahkan ke `FormKasir` (mesin kasir).
- **Akun nonaktif ditolak.** Kolom `is_active` dibaca saat login; akun yang
  dinonaktifkan admin tetap diberi pesan jelas, bukan "password salah".
- Menggunakan **parameterized query** (`@username`, `@password`) untuk mencegah SQL Injection.
- Waktu respons login disamakan untuk username ada dan tidak ada, supaya tidak
  bisa dipakai menebak username mana yang terdaftar.
- Otomatis mengosongkan field password dan memfokuskan kolom username saat login gagal.

### 2. Dashboard Admin

Tiga tombol aksi:

- **INPUT DATA BARANG** untuk membuka `FormBarang`.
- **BUKA MESIN KASIR** untuk membuka `FormKasir`.
- **LOGOUT (KELUAR)** untuk menutup dashboard dan kembali ke layar login.

### 3. Kelola Data Barang (CRUD)

Lengkap dengan operasi CRUD untuk tabel `tb_barang`:

- **Create (Simpan)**: menyimpan barang baru dengan kode barcode, nama, harga beli, harga jual, dan stok. Harga beli bersifat opsional (default `0` jika dikosongkan). Stok awal otomatis dicatat sebagai mutasi `MASUK`.
- **Read (Tampil)**: barang aktif dimuat ke `DataGridView` saat form dibuka, lengkap dengan kolom **Satuan**.
- **Update (Perbarui)**: memilih baris di tabel otomatis mengisi form input. Kode barcode dikunci (non-editable) saat mode edit. Bila stok diubah, selisihnya dicatat otomatis sebagai mutasi `PENYESUAIAN` sehingga tahu itu koreksi opname, bukan penjualan.
- **Hapus**: memakai **soft delete**. Tombol HAPUS mengubah `is_active` menjadi `0`, bukan menghapus baris. Barang berhenti muncul di daftar dan tidak bisa dijual, tetapi riwayat transaksi lamanya tetap utuh. Alasan disengaja, lihat bagian [Foreign Key](#struktur-tabel).
- **Aktifkan kembali**: tekan SIMPAN lalu isi kode barcode yang sama, aplikasi menanyakan apakah barang dinonaktifkan itu mau diaktifkan lagi.
- **Bersihkan**: mengosongkan seluruh kolom input dan membuka kembali kunci kode barcode.

Validasi: kode, nama, harga jual, dan stok wajib diisi sebelum disimpan.
Stok hanya menerima **bilangan bulat** per satuan, misalnya `10` pcs. Kolomnya
memang bertipe `NUMERIC` di database, tapi input dari keyboard dikunci digit
saja; koma dan titik tidak bisa diketik sama sekali. Alasannya, kesalahan
"1,5" yang terbaca "15" adalah 15 kali lipat, dan terlalu berbahaya untuk
dibiarkan lewat tebakan.

### 4. Mesin Kasir dan Transaksi

- **Scan barcode**: mendukung barcode scanner USB (berperilaku seperti keyboard). Kode dibaca melalui event `Enter` pada kolom barcode.
- **Pencarian otomatis**: barang dicari berdasarkan `kode_barcode` **dan harus berstatus aktif**. Barang yang dinonaktifkan akan ditolak walau barcode-nya masih tersimpan.
- **Keranjang belanja**: kolom Kode, Nama Barang, Harga, Qty, Subtotal.
  - Jika barcode yang sama di-scan berulang, **qty otomatis bertambah** tanpa membuat baris baru.
- **Ubah jumlah dengan klik kanan**: klik kanan pada baris keranjang untuk membuka
  menu **Ubah Jumlah...** atau **Hapus Item**. Menu ubah jumlah hanya menerima
  bilangan bulat, dan sekaligus memeriksa ulang stok terbaru dari database.
  Pecahan ditolak dengan pesan yang menyebutkan berapa yang sebenarnya diketik.
- **Total belanja**: dihitung otomatis setiap kali keranjang berubah. Subtotal
  dibulatkan ke 2 desimal agar angka di layar sama persis dengan yang tersimpan.
- **Kembalian otomatis**: dihitung realtime saat uang bayar diketik.
- **Pemeriksaan pembayaran**: transaksi ditolak jika keranjang kosong atau uang bayar kurang dari total.
- **Nomor nota otomatis**: format `TRX-yyyyMMddHHmmssfff` (dengan milidetik), contoh `TRX-20260927143022456`.
- **Penyimpanan transaksi ke 4 tabel** dalam satu transaksi database:
  1. Header ke `tb_transaksi`, termasuk `uang_diterima` dan `kembalian`.
  2. Rincian item ke `tb_detail_transaksi`, termasuk snapshot `nama_barang` dan `harga_satuan`.
  3. Pengurangan stok: `UPDATE tb_barang SET stok = stok - qty`.
  4. Riwayat stok ke `tb_mutasi_stok` bertipe `KELUAR` dengan qty negatif.
- **Layar otomatis bersih** setelah transaksi berhasil, siap untuk transaksi berikutnya.

---

## Teknologi

| Komponen | Versi / Keterangan |
|---|---|
| Bahasa | C# |
| Framework | .NET 10 (`net10.0-windows`) |
| UI | Windows Forms (WinForms) |
| Database | **SQLite** (satu berkas, tanpa server) |
| Connector | `Microsoft.Data.Sqlite` 10.0.12 (SQLite 3.53.3) |
| UI Library | `ReaLTaiizor` 3.8.2.1 (tema `HopeButton`) |
| IDE | Visual Studio 2022 / Visual Studio Code |

Tidak ada paket tambahan untuk hashing: password memakai **PBKDF2-SHA256** yang sudah tersedia di .NET.

Tidak ada lagi MySQL, MariaDB, XAMPP, Workbench, `mysql.exe`, username, password
database, port 3306, maupun service yang harus dijalankan. Seluruh data ada di
satu berkas bernama `data/kasir.db`.

---

## Prasyarat Instalasi

Sebelum menjalankan aplikasi, pastikan sudah tersedia:

1. **Windows 10/11** (aplikasi ini hanya berjalan di Windows).
2. **[.NET 10 SDK](https://dotnet.microsoft.com/download)** untuk proses build.
   Untuk menjalankan hasil build yang sudah jadi, cukup
   [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/download).
3. **Barcode scanner USB** (opsional) untuk transaksi cepat. Kode barcode juga dapat diketik secara manual.

> **Kalau akan memakai flashdisk FAT32:** SQLite tidak boleh memakai
> `journal_mode=WAL`, karena FAT32 tidak mendukung file journal dan
> file locking-nya tidak lengkap. Aplikasi ini tidak pernah menyalakan WAL,
> dan memakai `Cache=Private` supaya tidak muncul berkas `-wal` maupun `-shm`.
> Jangan menggantinya dengan WAL.

---

## Setup Database

### Tidak ada langkah setup sama sekali

Skema database dibentuk sendiri oleh aplikasi saat pertama kali dibuka.
Tidak ada perintah yang perlu dijalankan, tidak ada akun database yang perlu
dibuat, dan tidak ada tool eksternal yang perlu dipasang.

Yang terjadi pada proses startup:

1. `AppConfig` menentukan path berkas database (lihat
   [Konfigurasi Database](#konfigurasi-database)) dan membuat folder induknya
   bila belum ada.
2. `Database.PastikanTerpasang` membaca berkas `skema.sqlite.sql` yang ikut
   disertakan, memecahnya menjadi 20 perintah SQL, lalu menjalankannya
   **di dalam satu transaksi**. Kalau ada perintah yang gagal, tidak ada
   tabel yang tertinggal setengah jadi.
3. `tb_user` masih kosong, jadi satu akun admin dibuat memakai salt acak.

Akun admin bawaan:

| Username | Password | Role |
|---|---|---|
| `admin` | `admin123` | `Admin` |

> **GANTI password `admin123` sebelum dipakai di kasir sungguhan.**

### Verifikasi cepat

Jalankan aplikasi sekali, lalu pastikan folder `data` muncul di samping
berkas `.exe`:

```
data/
└── kasir.db        <-- satu-satunya berkas. Tidak ada -wal atau -shm.
```

Kalau `data` tidak muncul, berarti folder aplikasi tidak bisa ditulis.
Pesan errornya sudah ditampilkan di layar bersama lokasi berkasnya.

### Memasang skema secara manual (opsional)

Kalau ingin membuat `kasir.db` tanpa membuka aplikasi, misalnya untuk
menyiapkan data contoh sebelum diserahkan ke kasir:

```bash
sqlite3 data/kasir.db < skema.sqlite.sql
```

Perintah ini hanya membuat tabel. Akun admin **tidak** ikut, karena hash
password-nya sengaja tidak ditulis mati di dalam berkas SQL: siapa pun yang
membaca berkas itu bisa mengambil hash-nya lalu mencobanya secara offline.
Jalankan aplikasi sekali setelahnya, dan `admin` akan dibuat otomatis.

### Kalau Anda masih punya database MySQL lama

Data lama **tidak** dimigrasi. Aplikasi yang sudah dipindah ke SQLite mulai dari
nol, jadi tidak ada yang perlu dikonversi.

Skrip migrasi MySQL dan dump `mysqldump`-nya **sudah dihapus** dari folder
proyek pada 28 September 2026. Kalau datanya masih Anda perlukan, ekspor
**sekarang juga**, selagi XAMPP masih ada di komputer itu:

```bash
mysqldump -u root --default-character-set=utf8mb4 db_kasir_smk4 > arsip_lama.sql
```

Setelah XAMPP dihapus, data lama tidak bisa dibaca lagi tanpa reinstall MySQL.

---

## Struktur Tabel

Database terdiri dari 5 tabel dan 2 view. Diagram relasinya:

```
tb_user  ──────┬──< tb_transaksi ────< tb_detail_transaksi >──── tb_barang
                │                            │                        │
                │                            │                        │
                └────< tb_mutasi_stok >───────┴────────────────────────┘
```

### Ringkasan Tabel

| Tabel | Isi | Baris | Kunci |
|---|---|---|---|
| `tb_user` | Akun pengguna | 1 | `id_user` PK, `username` UNIQUE |
| `tb_barang` | Master barang dan stok | 2 | `kode_barcode` PK |
| `tb_transaksi` | Header nota penjualan | 1 | `id_transaksi` PK, `no_nota` UNIQUE |
| `tb_detail_transaksi` | Rincian item per nota | 1 | `id_detail` PK |
| `tb_mutasi_stok` | Riwayat perubahan stok | 1 | `id_mutasi` PK |

### Foreign Key (5 buah)

| Constraint | Dari | Ke | Aturan saat dihapus |
|---|---|---|---|
| `fk_transaksi_user` | `tb_transaksi.id_user` | `tb_user.id_user` | `RESTRICT` |
| `fk_detail_transaksi` | `tb_detail_transaksi.id_transaksi` | `tb_transaksi.id_transaksi` | `CASCADE` |
| `fk_detail_barang` | `tb_detail_transaksi.kode_barcode` | `tb_barang.kode_barcode` | `RESTRICT` |
| `fk_mutasi_barang` | `tb_mutasi_stok.kode_barcode` | `tb_barang.kode_barcode` | `RESTRICT` |
| `fk_mutasi_user` | `tb_mutasi_stok.id_user` | `tb_user.id_user` | `SET NULL` |

Artinya:

- Hapus satu nota, rincian itemnya **ikut terhapus** otomatis. Tidak ada data yatim.
- Hapus user atau barang yang **sudah dipakai transaksi akan ditolak** database.
  Ini alasan kenapa aplikasi memakai *soft delete* (`is_active`) dan bukan `DELETE`.

### CHECK Constraint (13 buah)

| Constraint | Aturan |
|---|---|
| `chk_user_role` | `role IN ('Admin','Kasir')` |
| `chk_barang_harga_beli` | `harga_beli >= 0` |
| `chk_barang_harga_jual` | `harga_jual > 0` |
| `chk_barang_stok` | `stok >= 0` |
| `chk_barang_grosir` | `minimal_grosir >= 0` dan `harga_grosir >= 0` |
| `chk_transaksi_total` | `total_bayar >= 0` |
| `chk_transaksi_diterima` | `uang_diterima >= 0` |
| `chk_transaksi_kembali` | `kembalian >= 0` |
| `chk_transaksi_diskon` | `diskon_total >= 0` |
| `chk_detail_qty` | `qty > 0` |
| `chk_detail_harga_satuan` | `harga_satuan >= 0` |
| `chk_detail_subtotal` | `subtotal >= 0` |
| `chk_detail_diskon` | `diskon_item >= 0` |

Stok negatif atau qty nol tidak bisa masuk ke database, bahkan kalau ada program
lain atau phpMyAdmin yang kelewat mengetik query secara manual.

### View (2 buah)

| View | Kegunaan | Contoh query |
|---|---|---|
| `v_laporan_penjualan` | Penjualan per baris detail, sudah termasuk nama kasir | `SELECT * FROM v_laporan_penjualan WHERE DATE(tanggal) = CURDATE()` |
| `v_stok_gudang` | Persediaan beserta nilai uangnya | `SELECT nama_barang, stok, nilai_persediaan FROM v_stok_gudang WHERE is_active = 1` |

Kedua view ini tidak dipakai kode aplikasi, tapi berguna untuk membuat laporan
cukup dengan satu `SELECT`.

Contoh laporan omzet harian:

```sql
SELECT DATE(tanggal) AS tanggal,
       COUNT(DISTINCT no_nota) AS jumlah_nota,
       SUM(subtotal) AS omzet
FROM v_laporan_penjualan
WHERE DATE(tanggal) >= '2026-09-01'
GROUP BY DATE(tanggal)
ORDER BY tanggal DESC;
```

Contoh laporan barang paling laku:

```sql
SELECT nama_barang,
       SUM(qty) AS total_qty,
       SUM(subtotal) AS total_penjualan
FROM v_laporan_penjualan
WHERE DATE(tanggal) BETWEEN '2026-09-01' AND '2026-09-30'
GROUP BY nama_barang
ORDER BY total_qty DESC
LIMIT 10;
```

Contoh rekonsiliasi kas (kasih ke supervisor):

```sql
SELECT tanggal,
       SUM(uang_diterima) AS total_masuk,
       SUM(kembalian)     AS total_kembalian,
       SUM(total_bayar)   AS total_penjualan
FROM tb_transaksi
WHERE DATE(tanggal) = CURDATE()
GROUP BY tanggal;
```

### Contoh Mutasi Stok

```sql
SELECT m.tanggal, b.nama_barang, m.tipe, m.qty, m.stok_akhir, m.keterangan
FROM tb_mutasi_stok m
JOIN tb_barang b ON b.kode_barcode = m.kode_barcode
WHERE m.kode_barcode = '123'
ORDER BY m.tanggal DESC, m.id_mutasi DESC;
```

Ini jawaban atas pertanyaan "kenapa stok barang 123 berubah?".

---

## Konfigurasi Database

Tidak ada lagi username, password, host, atau port. Yang dikonfigurasi hanyalah
**path berkas database**, dibaca dari `appsettings.json` yang ikut tersalin ke
folder output saat build.

```json
{
  "Database": {
    "Path": "data\\kasir.db"
  }
}
```

| Bagian | Arti | Nilai standar |
|---|---|---|
| `Database.Path` | Path ke berkas `.db`, relatif terhadap folder aplikasi | `data\kasir.db` |

Path relatif selalu diselesaikan terhadap folder berkas `.exe`, jadi seluruh
folder aplikasi bisa disalin ke flashdisk dan langsung jalan.

Urutan pembacaan konfigurasi:

1. Environment variable `KASIR_SMK4_DB` (path penuh ke berkas `.db`;
   menang atas berkas konfigurasi, berguna untuk pengujian).
2. `appsettings.json` di folder aplikasi.
3. Nilai bawaan `data/kasir.db` di samping berkas executable.

> Environment variable **`KASIR_SMK4_CONNECTION` sudah tidak berlaku**. Yang
> menggantikannya adalah `KASIR_SMK4_DB`, dan isinya bukan connection string
> melainkan path ke berkas.

### Connection string yang dihasilkan

`AppConfig.GetConnectionString()` menyusunnya sendiri. Dua bagiannya wajib
dipixelkan:

| Bagian | Nilai | Alasan |
|---|---|---|
| `Data Source` | path absolut ke `kasir.db` | satu berkas, tanpa server |
| `Pooling` | `False` | dengan pooling, berkas tetap terkunci beberapa detik setelah aplikasi ditutup, dan berkas yang terkunci tidak bisa disalin ke flashdisk |
| `Cache` | `Private` | mencegah SQLite membuat berkas `-wal` / `-shm` di samping database |
| `Mode` | `ReadWriteCreate` | membuat `kasir.db` otomatis saat pertama kali dijalankan |
| `Foreign Keys` | `True` | batasan referensial aktif (see catatan di bawah) |

### PRAGMA per-koneksi

`Koneksi.GetConn()` menjalankan dua PRAGMA pada **setiap** koneksi yang dibuat,
bukan sekali di awal aplikasi. PRAGMA bersifat per-koneksi, jadi kalau hanya
dipasang sekali, koneksi berikutnya kembali ke bawaan.

| PRAGMA | Nilai | Alasan |
|---|---|---|
| `foreign_keys` | `ON` | tanpa ini, penghapusan barang meninggalkan mutasi dan nota yatim |
| `recursive_triggers` | `OFF` | trigger cap waktu melakukan UPDATE ke tabelnya sendiri; kalau nyala akan memanggil dirinya sendiri tanpa henti |
| `busy_timeout` | 5000 ms | menahan sebentar saat berkas sedang dipakai proses lain, supaya flashdisk lambat tidak langsung gagal |

Karena PRAGMA hanya berlaku pada koneksi tempat ia dijalankan, **semua akses
ke database wajib lewat `Koneksi.GetConn()`**. Koneksi yang dibuat langsung
dengan `new SqliteConnection(...)` tidak punya setelan tersebut.

### Password User

Password **tidak lagi disimpan sebagai teks biasa**. Aplikasi menyimpan password dalam bentuk
hash **PBKDF2-SHA256** dengan 100.000 iterasi dan salt acak, dengan format:

```
PBKDF2$iterasi$saltBase64$hashBase64
```

**Catatan penting untuk data lama:** password lama yang masih berupa teks biasa **tetap bisa
login**. Begitu user tersebut login berhasil, password-nya otomatis di-upgrade menjadi hash.
Jadi Anda **tidak perlu** mengacak ulang seluruh password secara manual.

---

## Cara Menjalankan

### Opsi A: Visual Studio

1. Buka folder proyek, klik dua kali `AplikasiKasirSMK4.slnx`.
2. Tekan **Ctrl + F5** (Start Without Debugging) atau klik tombol Run.

Tidak ada layanan yang perlu dijalankan lebih dulu. `data\kasir.db` dibuat
otomatis saat jendela login tampil.

### Opsi B: Command Line (dotnet CLI)

```bash
# Restore paket NuGet
dotnet restore

# Build
dotnet build

# Jalankan
dotnet run
```

### Opsi C: Publish ke Folder Siap Salin ke Flashdisk

```bash
dotnet publish -c Release -r win-x64 --self-contained false
```

Folder hasil publish di `bin\Release\net10.0-windows\win-x64\` bisa langsung
disalin ke flashdisk. Yang perlu ikut disalin:

- semua `.exe` dan `.dll`
- `skema.sqlite.sql`
- `appsettings.json`
- folder `runtimes\win-x64\native\e_sqlite3.dll`

> **Jangan lupa `runtimes\`.** Driver SQLite memuat pustaka native
> `e_sqlite3.dll`. Folder `runtimes` bawaan hasil `dotnet build` juga berisi
> pustaka untuk Linux, macOS, dan ARM, yang tidak pernah dipakai. Setelah
> publish, hapus semua isi `runtimes\` kecuali `win-x64` supaya ukuran folder
> turun dari sekitar 20 MB menjadi sekitar 2 MB.

### Menjalankan dari flashdisk

1. Salin folder hasil publish ke flashdisk.
2. Jalankan `AplikasiKasirSMK4.exe` dari flashdisk.
3. Folder `data` dibuat otomatis di samping executable, dan `kasir.db` muncul
   di sana.

Tidak ada yang perlu diinstal, dan flashdisk boleh dilepas saat aplikasi
menutup. `Pooling=False` pada connection string memastikan berkas `kasir.db`
tidak terkunci setelah jendela ditutup, jadi flashdisk bisa langsung dicabut
atau `kasir.db` bisa langsung disalin sebagai cadangan.

---

## Akun Default

| Username | Password | Role | Akses |
|---|---|---|---|
| `admin` | `admin123` | Admin | Dashboard admin: kelola barang, buka kasir, laporan penjualan, logout |

> **Ganti password `admin123` sebelum aplikasi dipakai di kasir sungguhan.**

Hanya satu akun yang dibuat otomatis, yaitu `admin`. Akun kasir dibuat dari
dalam aplikasi lewat menu **Kelola User**, tidak lagi ditanam bersama
database. Akun `kasir` / `kasir123` yang ada di versi MySQL **tidak** ikut,
karena data lama tidak dimigrasi.

Password tidak disimpan sebagai teks biasa melainkan sebagai hash
**PBKDF2-SHA256** bersalt acak, 100.000 iterasi, dengan format:

```
PBKDF2$iterasi$saltBase64$hashBase64
```

Karena hash memakai salt acak, akun `admin` di dua instalasi berbeda tidak
memiliki hash yang sama, sehingga satu `kasir.db` yang bocor tidak langsung
membuka password instalasi lain.

Role yang diterima database hanya `Admin` dan `Kasir`. Nilai lain ditolak
oleh CHECK constraint `chk_user_role`. Daftar ini harus selalu sama dengan
`Session.IsRoleValid()` di `Form1.cs`.

---

## Alur Penggunaan

```
Buka Aplikasi
     |
     v
+-----------------+
|  Layar LOGIN   |   <-- pastikan skema terpasang otomatis
|    (Form1)      |
+--------+--------+
         | login berhasil
         v
   +-----------+
   | Cek Role |
   +-----+-----+
         |
    +----+----+
    v         v
+--------+  +------------------+
| ADMIN  |  |      KASIR       |
|FormMenu|  |    (FormKasir)   |
+---+----+  +--------+---------+
    |                |
    +- Input Barang  +- Scan barcode
    |   (FormBarang) |   --> isi keranjang
    |   CRUD stok    |
    |                +- Input uang bayar
    |                |   --> hitung kembalian
    |                |
    |                +- BAYAR & SIMPAN
    |                |   --> simpan transaksi
    |                |   --> kurangi stok
    |                |
    |                +- Tampilkan nomor nota
    |
    +- LOGOUT --> kembali ke Form1
```

### Skenario 1: Melakukan Transaksi

1. Login sebagai **Admin**, lalu klik **BUKA MESIN KASIR**.
2. Arahkan kursor ke kolom barcode (otomatis terfokus).
3. Scan barcode barang, atau ketik kodenya lalu tekan `Enter`.
4. Ulangi untuk setiap barang. Scan barang yang sama akan menambah qty, bukan membuat baris baru.
5. Untuk jumlah yang bukan 1, atau untuk barang timbang, **klik kanan** baris keranjang lalu pilih **Ubah Jumlah...**. Isi misalnya `0,5` atau `2,5`. Angka exceeding stok akan ditolak.
6. Pantau **TOTAL BELANJA** yang terupdate otomatis.
7. Masukkan nominal uang bayar, lalu **KEMBALIAN** terhitung otomatis.
8. Klik **BAYAR & SIMPAN**. Nomor nota, total, uang dibayar, dan kembalian akan ditampilkan.
9. Stok di gudang otomatis berkurang dan tercatat di riwayat mutasi stok.
10. Layar siap untuk transaksi berikutnya.

### Skenario 2: Mengelola Data Barang

1. Login sebagai **Admin**, lalu klik **INPUT DATA BARANG**.
2. Isi form: kode barcode, nama, harga beli (opsional), harga jual, stok. Stok boleh pecahan.
3. Klik **SIMPAN**. Stok awal otomatis dicatat sebagai mutasi `MASUK`.
4. Untuk mengubah data, klik baris di tabel. Data otomatis masuk ke form input dan kode barcode terkunci.
5. Ubah data yang diperlukan, lalu klik **PERBARUI**. Bila stok diubah, perubahan itu dicatat sebagai mutasi `PENYESUAIAN`.
6. Untuk menonaktifkan, pilih baris, klik **HAPUS**, lalu konfirmasi **Ya**. Barang hilang dari daftar tapi riwayat transaksinya tetap ada.
7. Untuk mengembalikan barang yang dinonaktifkan: klik **SIMPAN**, isi kode barcode yang sama, lalu jawab **Ya** saat ditanya.
8. Klik **BERSIHKAN** untuk mengembalikan form ke kondisi awal.

### Skenario 3: Menonaktifkan User

Belum ada layar manajemen user, jadi dilakukan lewat SQL:

```sql
-- Nonaktifkan kasir (riwayat transaksinya tetap tersimpan)
UPDATE tb_user SET is_active = 0 WHERE username = 'kasir';

-- Aktifkan kembali
UPDATE tb_user SET is_active = 1 WHERE username = 'kasir';
```

Akun yang dinonaktifkan akan ditolak saat login dengan pesan
"Akun sudah dinonaktifkan oleh administrator".

---

## Struktur Proyek

```
KASIR_SMK4/
|-- AplikasiKasirSMK4.slnx          # Solution file
|-- AplikasiKasirSMK4.csproj        # Project file (.NET 10, WinForms)
|-- appsettings.json                # Konfigurasi path berkas database
|-- skema.sqlite.sql                # SKEMA AKTIF. Dipakai aplikasi saat instalasi.
|-- README.md                       # Dokumentasi ini
|-- backup/                         # Hanya snapshot kode, bukan dump database
|   `-- snapshots/                  # Salinan kode sebelum tiap perubahan besar
|       |-- tahap1_awal_20260927_232511/    # 30 berkas, kondisi pra-Tahap-1
|       `-- tahap1_final_20260928_002410/   # 25 berkas + CATATAN.txt
|
|-- Program.cs                      # Entry point + penanganan error global
|-- Koneksi.cs                      # Koneksi SQLite + PRAGMA wajib per-koneksi
|-- AppConfig.cs                    # Penentuan path berkas database
|-- Database.cs                     # Installer skema + pemicu updated_at + seed admin
|-- QueryHelper.cs                  # Pengganti MySqlDataAdapter
|-- SqliteError.cs                  # Kategori galat: Unik, Check, ForeignKey, Null
|-- Session.cs                      # Data user yang sedang login
|-- PasswordHasher.cs               # Hashing password PBKDF2-SHA256
|-- InputHelper.cs                  # Validasi dan format input nominal
|-- InputDialog.cs                  # Dialog kecil minta satu nilai dari user
|
|-- Form1.cs                        # Logika layar login
|-- Form1.Designer.cs               # Layout login
|-- Form1.resx                      # Resource login
|
|-- FormMenu.cs                     # Logika dashboard admin
|-- FormMenu.Designer.cs            # Layout dashboard
|-- FormMenu.resx                   # Resource dashboard
|
|-- FormBarang.cs                   # Logika CRUD barang
|-- FormBarang.Designer.cs          # Layout CRUD barang
|-- FormBarang.resx                 # Resource CRUD barang
|
|-- FormKasir.cs                    # Logika transaksi dan barcode
|-- FormKasir.Designer.cs           # Layout mesin kasir
|-- FormKasir.resx                  # Resource mesin kasir
|
|-- bin/                            # Hasil build
`-- obj/                            # File objek intermediate build
```

> Pola WinForms: setiap form terdiri dari 3 file, yaitu `X.cs` (logika atau business logic), `X.Designer.cs` (definisi tampilan), dan `X.resx` (resource). File `.Designer.cs` jangan diedit manual karena akan ditimpa Visual Studio.
>
> `InputDialog.cs` sengaja dibuat **tanpa file `.Designer.cs` dan `.resx`**. Dialog ini
> dibangun langsung di kode supaya tidak menambah 3 berkas lagi untuk jendela kecil
> yang hanya dipakai satu kali.

---

## Detail Perubahan Data

### 1. `Koneksi.cs` - Manajemen Koneksi

- `GetConn()`: mengembalikan objek `SqliteConnection` baru, lengkap dengan
  PRAGMA yang sudah dijalankan. Dipanggil dengan pola `using` di setiap form
  agar koneksi otomatis ditutup. **Tidak boleh diganti `new SqliteConnection`
  langsung**, karena PRAGMA hanya berlaku pada koneksi tempat dijalankan.
- `TestConnection(out string pesanError)`: mencoba membuka koneksi dan mengembalikan `true` atau `false` tanpa menampilkan MessageBox, sehingga pemanggil yang menentukan cara menampilkannya.
- Connection string disusun sendiri oleh `AppConfig.GetConnectionString()` dari
  path berkas. Tidak ada lagi username, password, host, atau port.

### 2. `Form1.cs` - Autentikasi

Query yang digunakan:

```sql
SELECT id_user, password, nama_lengkap, role, is_active
FROM tb_user WHERE username = @username LIMIT 1;
```

- Password diverifikasi dengan `PasswordHasher.Verify`, yang otomatis menangani password lama (plain text) maupun hash baru.
- Password lama yang berhasil login otomatis di-upgrade menjadi hash.
- Role yang tidak dikenali (`Admin` / `Kasir`) akan ditolak.
- Akun dengan `is_active = 0` ditolak dengan pesan "Akun sudah dinonaktifkan oleh administrator", dibedakan dari pesan "password salah".
- Data user yang berhasil login disimpan di `Session` agar tidak perlu login berulang kali.
- `Session.UserId` inilah yang dipakai saat menyimpan transaksi.

### 3. `FormBarang.cs` - CRUD Barang

| Aksi | Method | Perintah SQL |
|---|---|---|
| Tambah | `btnSimpan_Click` | `INSERT INTO tb_barang ... is_active = 1` |
| Tampil | `TampilData()` | `SELECT ... FROM tb_barang WHERE is_active = 1` |
| Ubah | `btnEdit_Click` | `UPDATE tb_barang SET ... WHERE kode_barcode = @kode` |
| Nonaktifkan | `btnHapus_Click` | `UPDATE tb_barang SET is_active = 0 WHERE ...` |
| Catat mutasi | `CatatMutasiStok` | `INSERT INTO tb_mutasi_stok ...` |
| Cek kode | `KodeSudahAda` / `KodeSudahDipakaiNonaktif` | `SELECT COUNT(*) FROM tb_barang WHERE ...` |
| Baca satuan lama | `BacaSatuan` | `SELECT satuan FROM tb_barang WHERE ...` |
| Isi form | `dgvBarang_CellClick` | Membaca baris tabel ke kolom input |
| Validasi | `AmbilInput` | Memeriksa kode, nama, harga jual, harga beli, dan stok |
| Reset form | `BersihkanForm()` | Mengosongkan kolom input |

Perlindungan tambahan:

- Kode barcode duplikat dicegah sebelum insert, dengan pesan yang jelas.
- `SqliteException` dengan `ErrorCode = 19` dan `SqliteExtendedErrorCode` 1555
  (nilai unik) ditangani khusus oleh `SqliteError`.
- Role non-Admin tidak dapat membuka form ini.
- Harga dan stok divalidasi sebagai angka, tidak negatif, dan tidak melebihi
  kapasitas `NUMERIC` (dibatasi dua desimal oleh CHECK `ROUND(x,2) = x`).
- Bila stok diubah lewat form PERBARUI, selisihnya dicatat ke `tb_mutasi_stok` bertipe `PENYESUAIAN` supaya perubahannya bisa diaudit.
- **Satuan tidak pernah tertimpa.** Form ini belum punya kolom input satuan, jadi
  `satuan` selalu dibaca ulang dari database lewat `BacaSatuan`. Kalau tidak begitu,
  mengetik kode barcode secara manual lalu menekan PERBARUI akan diam-diam mengubah
  satuan `kg` menjadi `pcs`. Pengujian grup O di bawah mengunci perilaku ini.

### 4. `FormKasir.cs` - Transaksi

| Fitur | Method | Keterangan |
|---|---|---|
| Scan barcode | `txtBarcode_KeyDown` | Trigger pada tombol `Enter`, beep dicegah |
| Cari barang | `CariDanMasukKeranjang` | Hanya barang aktif, validasi stok, qty bertambah |
| Menu klik kanan | `PasangMenuKlikKananKeranjang` | Mengpasang `ContextMenuStrip` di keranjang |
| Ubah jumlah | `UbahQtyBarisDipilih` | Menerima pecahan, cek stok terbaru |
| Hapus item | `HapusBarisDipilih` | Dipakai oleh menu klik kanan dan double click |
| Hitung total | `HitungTotalBelanja` | Menjumlahkan kolom subtotal ke `_totalBelanja` |
| Hitung kembalian | `PerbaruiKembalian` | `kembalian = uangBayar - total`, tanpa nilai negatif |
| Bayar | `btnBayar_Click` | Validasi keranjang, nominal, dan pembayaran cukup |
| Simpan | `SimpanTransaksi(uangDiterima, kembalian)` | `BEGIN` / `COMMIT` / `ROLLBACK` |
| Pembulatan | `BulatkanSubtotal` | `Math.Round` 2 desimal, agar layar = database |

**Transaksi database.** Seluruh penyimpanan dibungkus `SqliteTransaction` yang
dibuka dengan `BeginTransaction(deferred: false)`, artinya `BEGIN IMMEDIATE`.
Bila satu saja query gagal, semua perubahan dibatalkan dan stok tidak akan
berkurang sebagian.

**Penguncian baris.** SQLite tidak punya `SELECT ... FOR UPDATE`. Yang
dipakai adalah `BeginTransaction(deferred: false)`, yaitu `BEGIN IMMEDIATE`:
kunci tulis diambil sejak transaksi dibuka, bukan menunggu perintah pertama.
Ini mencegah race condition ketika dua kasir menjual barang yang sama pada saat
bersamaan.

**Validasi stok tiga lapis:**

1. Saat scan barcode: barang non-aktif ditolak, stok `0` ditolak, dan total qty di keranjang tidak boleh melebihi stok.
2. Saat ubah jumlah dari menu klik kanan: stok terbaru dibaca ulang dari database.
3. Saat pembayaran: stok dicek ulang dari database di dalam transaction.

Urutan query saat pembayaran:

```sql
-- 1. Header transaksi (id_user dari Session, bukan angka hardcode)
INSERT INTO tb_transaksi (no_nota, id_user, total_bayar, uang_diterima, kembalian)
VALUES (@noNota, @idUser, @totalBayar, @uangDiterima, @kembalian);

-- id_transaksi hasil insert diambil lewat SELECT last_insert_rowid()
-- pada koneksi yang sama, untuk dipakai di bawah

-- 2. Untuk setiap baris keranjang:
--    a. Cek stok (kunci sudah dipegang oleh BEGIN IMMEDIATE)
SELECT nama_barang, stok FROM tb_barang WHERE kode_barcode = @kode;

--    b. Simpan rincian item + snapshot nama & harga saat penjualan
INSERT INTO tb_detail_transaksi
  (id_transaksi, no_nota, kode_barcode, nama_barang, qty, harga_satuan, subtotal)
VALUES (@idTransaksi, @noNota, @kode, @nama, @qty, @harga, @subtotal);

--    c. Kurangi stok
UPDATE tb_barang SET stok = stok - @qty WHERE kode_barcode = @kode;

--    d. Catat riwayat stok (qty negatif = keluar)
INSERT INTO tb_mutasi_stok
  (kode_barcode, tipe, qty, stok_akhir, keterangan, id_user, ref_no_nota)
VALUES (@kode, 'KELUAR', @qty, @stokAkhir, @keterangan, @idUser, @noNota);
```

**Kenapa `nama_barang` dan `harga_satuan` disimpan ulang di tabel detail?**
Karena keduanya adalah snapshot. Nilai yang ditulis adalah nilai **pada saat penjualan**.
Kalau nanti barang `123` diganti nama menjadi "Buku Tulis 38mm" atau harganya
dinaikkan dari 5.000 menjadi 7.500, nota lama dari bulan lalu tetap menampilkan
nama dan harga yang benar. Kolom `nama_barang` di `tb_barang` yang berubah,
bukan kolom salinan di `tb_detail_transaksi`.

### 5. `InputHelper.cs` - Validasi Input

Menghemat penulisan kode validasi di setiap form, dan mendukung format input Indonesia:

| Input | Hasil |
|---|---|
| `3000` | 3000 |
| `3.000` atau `30,000` | 3000 (dibaca sebagai pemisah ribuan) |
| `1.250.000` | 1250000 (ribuan bertingkat) |
| `2500.50` atau `2500,50` | 2500,50 (dibaca sebagai desimal) |
| `1.250,50` atau `1,250.50` | 1250,50 (campuran ribuan dan desimal) |
| `0,5` | 0,5 (qty pecahan) |
| `10.000,123` | **Ditolak** (lihat catatan di bawah) |
| `abc`, `-500`, `Rp 5000` | Ditolak dengan pesan jelas |

> **Kenapa `10.000,123` ditolak?**
> Input itu ambigu. `10.000,123` bisa berarti sepuluh ribu seratus dua puluh tiga,
> tapi bisa juga berarti sepuluh juta. Kalau tebakan salah, selisihnya **1000 kali
> lipat** di mesin kasir. Karena kolom nominal di database hanya punya 2 desimal,
> input dengan 3 desimal memang tidak pernah dipakai, jadi lebih aman ditolak
> dengan pesan yang jelas daripada ditebak.

Untuk output ada dua format:

| Fungsi | Digunakan untuk | Contoh |
|---|---|---|
| `FormatNominal` | Uang (harga, subtotal, total, kembalian) | `12500` → `12.500`, `2500.5` → `2.500,50` |
| `FormatJumlah` | Jumlah barang (qty, stok) | `1` → `1`, `1.5` → `1,5`, `0.25` → `0,25` |

`FormatJumlah` sengaja tidak memakai `FormatNominal` untuk kolom stok, supaya
`99` tampil sebagai `99` dan bukan `99,00`.

### 6. `PasswordHasher.cs` - Keamanan Password

- Algoritma PBKDF2-SHA256, 100.000 iterasi, salt acak 128 bit, kunci 256 bit.
- Perbandingan memakai `CryptographicOperations.FixedTimeEquals` (waktu konstan).
- Format: `PBKDF2$iterasi$saltBase64$hashBase64`.
- Password lama (plain text) tetap diverifikasi, lalu otomatis di-upgrade saat login berhasil.

### 7. `Session.cs` - Sesi dan Hak Akses

- `Session` menyimpan `UserId`, `Username`, `FullName`, dan `Role` selama aplikasi berjalan.
- `FormMenu` hanya dapat dibuka oleh role `Admin`.
- `FormBarang` hanya dapat dibuka oleh role `Admin`.
- Membuka jendela yang sama dua kali dicegah agar tidak terjadi tabrakan data.

### 8. `Program.cs` - Siklus Hidup Aplikasi

- `Program.cs` memasang penanganan error global, sehingga error yang tidak terduga tidak lagi membuat aplikasi crash diam-diam.
- `Form1` disembunyikan setelah login berhasil.
- Menutup form login dengan tombol X saat belum login akan menutup aplikasi sepenuhnya.
- Menutup dashboard dengan tombol X akan mengakhiri sesi dan menghentikan aplikasi.
- `FormKasir` yang dibuka langsung oleh role `Kasir` menjadi form utama, sehingga menutupnya juga menghentikan aplikasi.

---

## Restrukturasi Database (zaman MySQL)

> **Status: sudah diterapkan, lalu digantikan, lalu dihapus.**
> Bagian ini adalah catatan historis tentang perbaikan struktur yang dulu
> dilakukan di atas MySQL. Skrip yang dipakai waktu itu
> (`migration_pecahan.sql`, `migration_restruktur.sql`,
> `migration_id_unsigned.sql`, `skema_database.sql`) dan dump `mysqldump` di
> folder `backup/` **sudah dihapus** pada 28 September 2026, karena tidak ada
> lagi yang membutuhkannya.
>
> Satu-satunya salinannya masih ada di
> `backup\snapshots\tahap1_awal_20260927_232511\`, kalau someday benar-benar
> dibutuhkan untuk menelusuri asal-usul struktur.
>
> Untuk instalasi baru, yang dipakai adalah `skema.sqlite.sql`, dan installer
> membentuknya sendiri saat aplikasi dibuka. Lihat
> [Tahap 1: Dari MySQL ke SQLite](#tahap-1-dari-mysql-ke-sqlite).

Struktur yang dihasilkan restrukturasi MySQL itu **dipertahankan seluruhnya**
di skema SQLite: nama tabel, nama kolom, urutannya, 10 index, 5 foreign key,
CHECK constraint, 2 view, dan 1 trigger. Yang berubah hanya mesinnya.

### Masalah pada Struktur Lama

Struktur awal database terlihat rapi, tapi sebenarnya rapuh. Berikut temuan
selama peninjauan:

| No | Masalah | Akibatnya |
|---|---|---|
| 1 | **Nol foreign key** di seluruh database | Baris transaksi bisa menunjuk user atau barang yang tidak ada. Menghapus user tidak menyentuh transaksinya, jadi data jadi tidak sinkron. |
| 2 | **Tanpa index** di kolom pencarian | `WHERE tanggal BETWEEN ...` melakukan *full table scan*. Lambat sekali ketika transaksi sudah ratusan ribu baris. |
| 3 | **`tb_transaksi` tanpa kunci numerik** | `no_nota` VARCHAR jadi primary key. Sulit direferensikan, dan `LastInsertId` tidak tersedia. |
| 4 | **Harga satuan tidak disimpan** | `tb_detail_transaksi` hanya menyimpan `subtotal`. Kalau qty 3, harga jualnya berapa? Tidak bisa tahu. Laporan laba mustahil dibuat. |
| 5 | **Nama barang tidak disimpan** | Kalau nama barang diubah, nota lama ikut berubah. Nota jadi tidak sesuai kenyataan. |
| 6 | **Uang diterima & kembalian tidak disimpan** | Aplikasi menghitung kembalian di layar lalu membuangnya. Tidak ada cara mengetahui berapa uang yang benar-benar diterima untuk rekonsiliasi kas. |
| 7 | **Hapus barang permanen** | Barang yang pernah terjual hilang dari master, sementara riwayatnya masih ada. Laporan jadi tidak konsisten. |
| 8 | **Tidak ada audit trail stok** | Stok berubah dari 10 jadi 3, tidak ada yang bisa menjelaskan kenapa. |
| 9 | **`qty` dan `stok` bertipe INT** | Tidak bisa menjual 0,5 kg beras. |
| 10 | **Tanpa CHECK constraint** | Stok negatif atau qty nol bisa masuk lewat program lain atau ketikan manual. |
| 11 | **`role` ENUM** | Menambah role baru butuh `ALTER TABLE`. |
| 12 | **`tanggal` boleh NULL** | Laporan berdasarkan tanggal bisa menemukan baris tanpa tanggal. |

### Yang Diperbaiki

**1. Foreign key — dari 0 menjadi 5**

```
tb_transaksi.id_user        --> tb_user.id_user              (RESTRICT)
tb_detail_transaksi         --> tb_transaksi.id_transaksi    (CASCADE)
tb_detail_transaksi.kode    --> tb_barang.kode_barcode       (RESTRICT)
tb_mutasi_stok.kode         --> tb_barang.kode_barcode       (RESTRICT)
tb_mutasi_stok.id_user      --> tb_user.id_user              (SET NULL)
```

Hapus nota → rinciannya ikut terhapus. Hapus user atau barang yang sudah dipakai
transaksi → **ditolak** database. Inilah alasan aplikasi memakai soft delete.

**2. Index — 11 index baru**

| Tabel | Index | Untuk apa |
|---|---|---|
| `tb_barang` | `ix_barang_nama` | Pencarian berdasarkan nama |
| `tb_barang` | `ix_barang_aktif` | Daftar barang aktif yang sudah terurut |
| `tb_transaksi` | `ix_transaksi_tanggal` | Laporan per periode |
| `tb_transaksi` | `ix_transaksi_user` | Laporan per kasir |
| `tb_transaksi` | `uq_transaksi_no_nota` | Jaminan nomor nota unik |
| `tb_detail_transaksi` | `ix_detail_barang` | Rekap penjualan per barang |
| `tb_detail_transaksi` | `ix_detail_nota` | Pengambilan rincian per nota |
| `tb_user` | `ix_user_aktif` | Daftar user aktif |
| `tb_mutasi_stok` | `ix_mutasi_barang` | Riwayat stok satu barang |
| `tb_mutasi_stok` | `ix_mutasi_tanggal` | Riwayat stok per periode |
| `tb_mutasi_stok` | `ix_mutasi_tipe` | Filter jenis mutasi |

**3. Primary key numerik di `tb_transaksi`**

Sebelumnya `no_nota` VARCHAR(50) jadi primary key. Sekarang:

```sql
id_transaksi BIGINT UNSIGNED AUTO_INCREMENT   -- primary key, untuk foreign key
no_nota      VARCHAR(50) UNIQUE              -- nomor yang dilihat manusia
```

Kunci numerik membuat foreign key jauh lebih cepat, dan `no_nota` tetap ada
karena itu nomor yang dicetak di struk.

**4. Snapshot harga dan nama**

Ditambah dua kolom di `tb_detail_transaksi`:

| Kolom | Isi |
|---|---|
| `nama_barang` | Nama barang **pada saat penjualan** |
| `harga_satuan` | Harga satuan **pada saat penjualan** |

Untuk data lama, `harga_satuan` dihitung dari `subtotal / qty`, yaitu harga yang
benar-benar dibayar. Ini yang memungkinkan laporan "produk apa yang paling untung"
dibuat belakangan.

**5. Uang diterima dan kembalian**

Ditambah `uang_diterima` dan `kembalian` di `tb_transaksi`. Sekarang rekonsiliasi
kas akhir hari bisa dilakukan dengan satu query. Data lama diisi dengan asumsi
`uang_diterima = total_bayar` dan `kembalian = 0`.

**6. Soft delete**

Ditambah kolom `is_active` di `tb_barang` dan `tb_user`.

| Aksi | SQL | Akibat |
|---|---|---|
| Hapus barang | `UPDATE tb_barang SET is_active = 0` | Hilang dari daftar, tidak bisa dijual, riwayat utuh |
| Hapus user | `UPDATE tb_user SET is_active = 0` | Tidak bisa login, riwayat transaksinya tetap ada |

**7. Tabel baru `tb_mutasi_stok`**

Menjawab pertanyaan "kenapa stok barang ini berubah?".

| `tipe` | Kapan | `qty` |
|---|---|---|
| `MASUK` | Barang baru, atau barang diaktifkan kembali | positif |
| `KELUAR` | Terjual di kasir | negatif |
| `PENYESUAIAN` | Koreksi admin, mis. opname atau barang rusak | bisa + atau - |

Diisi oleh **kode aplikasi**, bukan trigger database, supaya logikanya terlihat
jelas di C# dan mudah ditelusuri saat debugging. Data transaksi lama direkonstruksi
otomatis saat migrasi.

**8. `qty` dan `stok` jadi `DECIMAL(15,2)`**

Ini yang membuat barang timbang bisa dijual. Di sisi aplikasi, `qty` berubah dari
`int` menjadi `decimal`, ditambah menu klik kanan untuk mengoreksi jumlah:

```
Klik kanan pada baris keranjang
  +-- Ubah Jumlah...  --> menerima pecahan, mis. 0,5
  `-- Hapus Item
```

Kolom `minimal_grosir` juga ikut jadi `DECIMAL(15,2)`.

**9. Constraint `CHECK` — 13 buah**

Mencegah stok negatif, qty nol, harga jual nol, role ngawur, dan nominal negatif,
bahkan kalau ada yang mengetik query manual di luar aplikasi.

**10. Jejak waktu dan satuan**

- `tb_user.dibuat_pada`, `tb_barang.dibuat_pada`, `tb_barang.diperbarui_pada`
- `tb_barang.satuan` dengan nilai bawaan `pcs`, untuk `pcs` / `kg` / `box`

**11. Tipe ID diseragamkan**

`id_user`, `id_transaksi`, `id_detail`, `id_mutasi` sekarang `UNSIGNED`, sehingga
nomor ID tidak mungkin bernilai negatif dan konsisten di semua tabel.

### Ringkasan Perubahan Kolom

| Tabel | Kolom | Sebelum | Sesudah |
|---|---|---|---|
| `tb_barang` | `stok` | `int(11)` | `decimal(15,2)` |
| `tb_barang` | `minimal_grosir` | `int(11) NULL` | `decimal(15,2) NOT NULL` |
| `tb_barang` | `harga_beli` | `int(11)` | `decimal(15,2)` |
| `tb_barang` | `harga_jual` | `int(11)` | `decimal(15,2)` |
| `tb_barang` | `harga_grosir` | `int(11) NULL` | `decimal(15,2) NOT NULL` |
| `tb_barang` | `satuan` | *tidak ada* | `varchar(20) NOT NULL DEFAULT 'pcs'` |
| `tb_barang` | `is_active` | *tidak ada* | `tinyint(1) NOT NULL DEFAULT 1` |
| `tb_barang` | `dibuat_pada`, `diperbarui_pada` | *tidak ada* | `timestamp` |
| `tb_user` | `role` | `enum('Admin','Kasir')` | `varchar(20)` + CHECK |
| `tb_user` | `is_active` | *tidak ada* | `tinyint(1) NOT NULL DEFAULT 1` |
| `tb_user` | `dibuat_pada` | *tidak ada* | `timestamp` |
| `tb_transaksi` | `id_transaksi` | *tidak ada* | `bigint unsigned` PK AUTO_INCREMENT |
| `tb_transaksi` | `no_nota` | PK varchar | `varchar(50) UNIQUE` |
| `tb_transaksi` | `tanggal` | `datetime NULL` | `datetime NOT NULL` |
| `tb_transaksi` | `uang_diterima` | *tidak ada* | `decimal(15,2) NOT NULL` |
| `tb_transaksi` | `kembalian` | *tidak ada* | `decimal(15,2) NOT NULL` |
| `tb_transaksi` | `catatan` | *tidak ada* | `varchar(255) NULL` |
| `tb_detail_transaksi` | `id_detail` | `int(11)` | `bigint unsigned` |
| `tb_detail_transaksi` | `id_transaksi` | *tidak ada* | `bigint unsigned` NOT NULL |
| `tb_detail_transaksi` | `nama_barang` | *tidak ada* | `varchar(150)` snapshot |
| `tb_detail_transaksi` | `harga_satuan` | *tidak ada* | `decimal(15,2)` snapshot |
| `tb_detail_transaksi` | `qty` | `int(11)` | `decimal(15,2)` |
| `tb_detail_transaksi` | `diskon_item` | `int(11) NULL` | `decimal(15,2) NOT NULL` |
| `tb_mutasi_stok` | *tabel baru* | *tidak ada* | 9 kolom, 2 foreign key |

### Cara Menjalankan Migrasi (historis, MySQL)

> **Tidak berlaku lagi.** Ketiga skrip migrasi MySQL sudah dihapus dari
> folder proyek pada 28 September 2026. Aplikasi versi sekarang tidak
> memerlukannya, dan data lama memang tidak dimigrasi.
>
> Untuk data aplikasi sekarang, cadangan cukup berupa **satu berkas**
> `data\kasir.db`, dan pemulihannya sesederhana menyalin berkas itu kembali ke
> tempatnya. Lihat [Cadangan dan Pemulihan](#cadangan-dan-pemulihan).

### Cara Restore Backup (historis, MySQL)

> Berkas dump `mysqldump` yang dulu ada di folder `backup/` juga sudah
> dihapus. Tidak ada lagi yang perlu dipulihkan dari MySQL.

### Bukti bahwa Migrasi Benar

Rantai migrasi diuji dari nol, bukan diasumsikan benar:

1. Database `db_uji_migrasi` dibuat dengan memulihkan backup struktur lama
   (4 tabel tanpa foreign key).
2. Ketiga skrip migrasi dijalankan berurutan. Semuanya keluar dengan kode 0
   dan tanpa satu pun peringatan.
3. Struktur hasilnya dibandingkan otomatis dengan instalasi bersih, di 89
   titik: definisi kolom (tipe, urutan, null, default), index, foreign key
   beserta aturan cascade, check constraint, engine, collation, dan daftar view.
   **Hasil: tidak ada perbedaan sama sekali.**
4. Seluruh pengujian aplikasi dijalankan terhadap database hasil migrasi
   tersebut. **Hasil: semua lulus, 0 gagal.**

Jadi database hasil migrasi dari struktur lama dan database dari instalasi baru
benar-benar identik, bukan hanya terlihat mirip.

### Verifikasi Hasil Restrukturasi

Seleksi berikut dipakai untuk memastikan semua constraint benar-benar bekerja:

```sql
-- Semua harus mengembalikan 0 baris
START TRANSACTION;
INSERT INTO tb_barang (kode_barcode, nama_barang, harga_jual, stok)
  VALUES ('NEG1', 'Uji Stok Negatif', 1000, -5);          -- ditolak: chk_barang_stok
ROLLBACK;

START TRANSACTION;
INSERT INTO tb_barang (kode_barcode, nama_barang, harga_jual, stok)
  VALUES ('NEG2', 'Uji Harga 0', 0, 10);                 -- ditolak: chk_barang_harga_jual
ROLLBACK;

START TRANSACTION;
INSERT INTO tb_transaksi (no_nota, id_user, total_bayar)
  VALUES ('TRX-FK', 999, 5000);                            -- ditolak: fk_transaksi_user
ROLLBACK;

START TRANSACTION;
DELETE FROM tb_barang WHERE kode_barcode = '123';         -- ditolak: fk_detail_barang
ROLLBACK;

-- Laporan integritas data
SELECT 'detail tanpa transaksi' AS cek, COUNT(*) AS jumlah
  FROM tb_detail_transaksi d
  LEFT JOIN tb_transaksi t ON d.no_nota = t.no_nota WHERE t.no_nota IS NULL
UNION ALL
SELECT 'detail tanpa barang', COUNT(*)
  FROM tb_detail_transaksi d
  LEFT JOIN tb_barang b ON d.kode_barcode = b.kode_barcode WHERE b.kode_barcode IS NULL
UNION ALL
SELECT 'transaksi tanpa user', COUNT(*)
  FROM tb_transaksi t
  LEFT JOIN tb_user u ON t.id_user = u.id_user WHERE u.id_user IS NULL
UNION ALL
SELECT 'harga satuan kosong', COUNT(*)
  FROM tb_detail_transaksi WHERE harga_satuan <= 0
UNION ALL
SELECT 'stok negatif', COUNT(*) FROM tb_barang WHERE stok < 0;
```

---

## Tahap 1: Dari MySQL ke SQLite

> **Status: selesai dan terverifikasi.** 289 tes lulus, 0 gagal.

### Alasan

Aplikasi ini harus bisa dipakai dari flashdisk di mana saja, termasuk komputer
yang tidak punya internet dan tidak pernah melihat XAMPP. Selama masih MySQL,
salin ke flashdisk berarti bawa folder htdocs, folder mysql, dan service
database yang harus jalan. Itu tidak mungkin di depan kasir.

### Apa yang berubah

| No | Aspek | MySQL (sebelum) | SQLite (sekarang) |
|---|---|---|---|
| 1 | Bentuk data | Folder berisi file server | **Satu file** `data\kasir.db` |
| 2 | Koneksi | Host, port, user, password | Tidak ada. Cuma path file |
| 3 | Paket NuGet | `MySql.Data` 26.7.0 | `Microsoft.Data.Sqlite` 10.0.12 |
| 4 | Cara pasang | Jalankan XAMPP, import SQL | Tidak ada, installer otomatis |
| 5 | Folder aplikasi | `htdocs` + folder project | Satu folder, bisa disalin |
| 6 | Tipe uang | `DECIMAL(15,2)` | `NUMERIC` + CHECK `ROUND(x,2) = x` |
| 7 | Tipe `TINYINT(1)` | Dibaca driver sebagai `bool` | `INTEGER` 0/1, jebakan hilang |
| 8 | Cap waktu update | `ON UPDATE CURRENT_TIMESTAMP` | Trigger `trg_barang_updated` |
| 9 | Kunci baris saat transaksi | `SELECT ... FOR UPDATE` | `BeginTransaction(deferred: false)` |
| 10 | Ambil id baris baru | `LAST_INSERT_ID()` | `SELECT last_insert_rowid()` |
| 11 | Kode galat | `1062`, `1452`, `1451`, `4025` | `19` + `SqliteExtendedErrorCode` |
| 12 | Hapus multi-tabel | `DELETE t FROM t JOIN ...` | `WHERE id IN (SELECT ...)` |
| 13 | Isi tabel | `MySqlDataAdapter` | `QueryHelper.IsiTabel` |
| 14 | Konfigurasi | `KASIR_SMK4_CONNECTION` | `KASIR_SMK4_DB` (path file) |
| 15 | Berkas skema | `skema_database.sql` (MySQL) | `skema.sqlite.sql` |

**Data lama tidak dimigrasi.** Aplikasi mulai dari nol. `tb_user` kosong, jadi
satu akun `admin` dibuat otomatis. Kalau data MySQL lama masih dibutuhkan,
ekspor dulu dengan `mysqldump` sebelum XAMPP dimatikan.

### Paritas skema, dibuktikan bukan dikira

Struktur hasil restrukturasi MySQL dipertahankan **seluruhnya**. Berikut
hasil perbandingan otomatis antara `skema.sqlite.sql` dengan skema MySQL lama:

| Objek | MySQL | SQLite | Status |
|---|---|---|---|
| Tabel | 5 | 5 | identik |
| Kolom | 45 | 45 | identik, urutan sama |
| Index | 10 | 10 | identik |
| Foreign key | 5 | 5 | identik |
| View | 2 | 2 | identik |
| CHECK constraint | 13 | 14 | +1 (lihat catatan ENUM di bawah) |
| Trigger | 0 | 1 | baru, menggantikan `ON UPDATE` |

Penambahan CHECK ke-14 adalah `chk_mutasi_tipe`, yang menggantikan tipe `ENUM`
pada kolom `tipe`. SQLite tidak punya tipe `ENUM`, jadi daftar nilainya ditulis
sebagai syarat. Isinya sama persis dengan daftar yang dulu di `ENUM`.

### Behavior SQLite yang harus diketahui

Bagian ini penting. Semuanya diukur langsung, bukan diaspora dari dokumentasi.

#### Uang bukan desimal presisi-tetap

SQLite menyimpan `NUMERIC` sebagai bilangan pecahan biner IEEE-754. Tidak ada
`DECIMAL(15,2)`. Artinya perkalian di dalam agregat bisa menghasilkan lebih dari
dua desimal. Angka di bawah hasil pengukuran di SQLite 3.53.3:

```
1235.16 * 0.1                    = 123,516
1235.16 * 0.2                    = 247,032
1235.16 * 0.3                    = 370,548
SUM(harga_satuan * qty)          = 741,096     <-- tiga desimal
ROUND(SUM(harga_satuan * qty),2) = 741,10
```

Tiga aturan yang muncul dari itu:

1. **Semua agregat uang wajib dibungkus `ROUND`.**
   Benar: `SELECT ROUND(SUM(total_bayar), 2) FROM ...`
   Salah: `SELECT SUM(total_bayar) FROM ...`

2. **Laporan menjumlahkan kolom `subtotal`, bukan mengalikan ulang.**
   `subtotal` sudah dibulatkan dua desimal saat nota disimpan oleh
   `FormKasir.BulatkanSubtotal`, dan dijaga CHECK `ROUND(x,2) = x`. Mengali
   ulang harga kali qty di dalam SQL menghasilkan 741,096 untuk total yang
   seharusnya 741,10.

3. **Aritmetika C# tetap pakai `decimal`.**
   `decimal` di C# eksak, jadi 1235.16 * 0.1 = 123.516 tepat. Nilainya tetap
   harus dibulatkan sebelum disimpan, dan CHECK constraint akan menolaknya
   kalau terlupa.

#### Tipe ENUM tidak ada

Diganti CHECK constraint. Daftar nilainya harus selalu sama dengan
`Session.IsRoleValid()` di `Form1.cs`. Menambah role berarti mengubah **dua**
tempat, bukan satu.

#### `ON UPDATE CURRENT_TIMESTAMP` tidak ada

Diganti trigger. Bentuk yang dipakai:

```sql
CREATE TRIGGER IF NOT EXISTS trg_barang_updated
AFTER UPDATE ON tb_barang
FOR EACH ROW
BEGIN
    UPDATE tb_barang
       SET diperbarui_pada = strftime('%Y-%m-%d %H:%M:%S', 'now', 'localtime')
     WHERE kode_barcode = NEW.kode_barcode;
END;
```

Dua jebakan yang sudah ditemukan dan dihindari:

- **`NEW.col = nilai` tidak didukung SQLite.** Akan muncul
  `near "NEW": syntax error`. Cara resmi adalah UPDATE atau INSERT bersarang
  seperti di atas.
- **`WHEN OLD.x IS NOT NEW.x` di trigger AFTER selalu bernilai FALSE.** Kolomnya
  masih menyimpan nilai lama saat syarat dicek, jadi triggernya terpasang tapi
  diam-diam tidak mengubah apa pun. Trigger ini sengaja tidak memakai `WHEN`.

Trigger `updated_at` juga harus punya `PRAGMA recursive_triggers = OFF`, karena
ia melakukan UPDATE ke tabelnya sendiri.

#### PRAGMA bersifat per-koneksi

Tidak bisa cukup dipasang sekali di awal aplikasi. `Koneksi.GetConn()`
mengulanginya pada setiap koneksi:

| PRAGMA | Nilai | Kalau tidak dipasang |
|---|---|---|
| `foreign_keys` | `ON` | Hapus barang meninggalkan mutasi dan nota yatim |
| `recursive_triggers` | `OFF` | Trigger updated_at memanggil dirinya sendiri tanpa henti |
| `busy_timeout` | 5000 ms | Flashdisk lambat langsung gagal |

Karena itu **semua akses database wajib lewat `Koneksi.GetConn()`**.

#### Pooling harus dimatikan

Dengan `Pooling=True`, berkas tetap terkunci beberapa detik setelah aplikasi
ditutup, dan muncul galat "being used by another process". Berkas yang
 terkunci tidak bisa disalin ke flashdisk. `Pooling=False` +
`Cache=Private` memastikan `kasir.db` bebas langsung disalin begitu jendela
ditutup.

#### Tidak boleh memakai WAL

`journal_mode=WAL` menghasilkan berkas `-wal` dan `-shm` di samping database.
FAT32 tidak mendukung file journal dan file locking-nya tidak lengkap, jadi WAL
tidak boleh dinyalakan untuk aplikasi yang disalin ke flashdisk. Aplikasi ini
tidak pernah menyetelnya.

#### Installer memecah sendiri berkas SQL

SQLite hanya menjalankan perintah pertama per panggilan `ExecuteNonQuery`.
Isi `skema.sqlite.sql` tidak bisa dijalankan sekaligus, jadi
`Database.PisahStatementSql` memecahnya sendiri. Pemecah itu harus mengenal:

- titik koma di dalam string
- dua hubung (`--`) di dalam string yang bukan komentar
- kutip yang diulang di dalam string
- string berpenanda kutip-ganda
- badan trigger (`BEGIN ... END`) yang memuat titik koma

Seluruh perintah dijalankan **di dalam satu transaksi**, jadi pemasangan yang
gagal di tengah tidak meninggalkan tabel setengah jadi.

### Yang sengaja tidak diubah

| Nilai lama | Nilai sekarang | Alasan |
|---|---|---|
| `AutoIncrement` | `AUTOINCREMENT` penuh | `id` = jejak audit, tidak boleh dipakai ulang |
| `TINYINT(1)` | `INTEGER` + CHECK | Hilangkan jebakan `bool` |
| `AUTO_INCREMENT` | `AUTOINCREMENT` | Untuk SQLite wajib ditulis penuh |
| Tidak ada tipe uang | `NUMERIC` + CHECK | Ganti `DECIMAL(15,2)` |

### Berkas baru

| Berkas | Isi |
|---|---|
| `skema.sqlite.sql` | Skema instalasi SQLite, 316 baris, ASCII-only, idempotent |
| `SqliteError.cs` | Mengubah `SqliteException` jadi kategori: `Unik`, `Check`, `ForeignKey`, `TidakBolehNull` |
| `QueryHelper.cs` | Pengganti `MySqlDataAdapter` untuk mengisi `DataTable` |
| `AppConfig.cs` | Path database + connection string |

### Cara menjalankan pengujian

Pengujian memakai **berkas database sendiri** di `data\kasir_uji.db` di folder
build, bukan `kasir.db` milik pengguna. Dipasthkan lewat environment variable
`KASIR_SMK4_DB` sebelum apa pun menyentuh `AppConfig`.

```powershell
cd ujiapp
dotnet build --no-incremental    # --no-incremental wajib
dotnet run --no-build
```

Hasil terakhir: **289 lulus, 0 gagal**. Harness membersihkan semua datanya
sendiri; setelah dijalankan, `kasir_uji.db` hanya berisi satu akun `admin`.

---

## Cadangan dan Pemulihan

### Membuat cadangan

Cadangan cukup menyalin **satu berkas**. Aplikasi harus sudah ditutup dulu,
supaya `kasir.db` tidak sedang ditulis.

```
1. Tutup jendela aplikasi
2. File Explorer -> folder aplikasi -> data
3. Klik kanan kasir.db -> Salin
4. Tempel ke flashdisk atau folder cadangan
5. Beri nama sesuai tanggal, misal kasir_2026-09-28.db
```

Kalau ingin lebih aman, salin ke **dua** tempat berbeda. Berkas ini satu-satunya
yang berisi data, jadi kehilangan berarti kehilangan semua penjualan.

> Menu backup dari dalam aplikasi **belum ada**. Sudah direncanakan untuk tahap
> berikutnya, bersama 16 fitur lain.

### Pemulihan

1. Tutup aplikasi.
2. Hapus `data\kasir.db` yang sekarang.
3. Salin berkas cadangan ke `data\kasir.db`.
4. Jalankan aplikasi.

Tidak ada perintah, tidak ada tool, tidak ada akun yang perlu dibuat.
Aplikasi langsung membaca salinan itu, apa adanya.

### Memeriksa cadangan

Tanpa aplikasi, pakai SQLiteStudio atau `sqlite3` dari terminal:

```bash
sqlite3 data/kasir.db "SELECT COUNT(*) FROM tb_transaksi;"
```

Kalau angkanya masuk akal, salinannya utuh. Kalau muncul `no such table` atau
`file is not a database`, berarti yang tersalin bukan `kasir.db` yang benar.

### Yang perlu dicadangkan, dan yang tidak

| Yang perlu dicadangkan | Tidak perlu |
|---|---|
| `data\kasir.db` | Berkas `.exe` dan `.dll` (diambil ulang dari installer) |
| | Folder `bin\` dan `obj\` (hasil build) |
| | Berkas `skema.sqlite.sql` (ikut di dalam installer) |

---

## Ringkasan Perbaikan

> **Bagian ini mencakup dua tahap.** Cacat 1 sampai 26 ditemukan pada versi
> awal yang masih MySQL. Cacat 27 sampai 33 ditemukan saat migrasi ke SQLite dan
> tidak mungkin ditemukan sebelum itu, karena penyebabnya adalah behavior
> SQLite yang berbeda.

Berikut daftar cacat yang ditemukan pada versi awal dan perbaikannya.

### Cacat Berat

| No | Cacat | Dampak | Perbaikan |
|---|---|---|---|
| 1 | Transaksi tanpa `BEGIN` / `COMMIT` | Kalau gagal di tengah, nota tersimpan tapi stok tidak berkurang, sehingga data jadi tidak sinkron | Semua query dibungkus `MySqlTransaction` dengan `Commit` dan `Rollback` |
| 2 | `id_user` hardcode `1` | Semua transaksi tercatat sebagai user ID 1, riwayat penjualan tidak bisa tahu siapa yang menjual | `id_user` diambil dari `Session.UserId` |
| 3 | Tidak ada validasi stok | Barang stok `0` tetap bisa dijual, dan stok bisa menjadi minus | Validasi saat scan dan saat pembayaran, memakai `SELECT ... FOR UPDATE` |
| 4 | Password plain text | Password mudah dibaca siapa pun yang punya akses file database | PBKDF2-SHA256 100.000 iterasi, salt acak, auto-upgrade dari password lama |
| 5 | Credential database di dalam kode | Credential ikut tersebar bersama source code | Dipindah ke `appsettings.json` |

### Cacat Ringan

| No | Cacat | Perbaikan |
|---|---|---|
| 6 | `Form1_Load` tidak pernah dipanggil, `Load` tertaut ke `Form1_Load_1` yang kosong | Sudah ditautkan ke `Form1_Load` yang benar, method kosong dihapus |
| 7 | `using AplikasiKasirSMK4;` di dalam file yang sama dengan namespace-nya | Dihapus |
| 8 | `CekKoneksi()` menampilkan MessageBox dari dalam class koneksi | Diganti `TestConnection(out string pesanError)`, pemanggil yang menampilkan |
| 9 | Field `conn` di `Koneksi` dipakai bersama antar form | Diganti variabel lokal, tidak ada lagi shared state |
| 10 | Tidak ada cara menghapus item dari keranjang | Double click pada baris keranjang untuk menghapus |
| 11 | Kembalian bisa tampil negatif | Dibatasi agar tidak menampilkan nilai negatif |
| 12 | Input angka tidak divalidasi, `Convert.ToInt32` bisa melempar exception | `InputHelper` dengan pesan error yang jelas |
| 13 | Kode barcode duplikat hanya menghasilkan error MySQL mentah | Dicegah sebelum insert dengan pesan yang jelas |
| 14 | Hapus barang gagal diam-diam jika ada foreign key | Ditangani `MySqlException` 1451 dengan pesan yang jelas |
| 15 | Menutup jendela bisa menyisakan proses berjalan tanpa jendela | Ditambahkan penanganan `FormClosing` di `Form1`, `FormMenu`, `FormKasir` |
| 16 | Error yang tidak tertangani membuat aplikasi crash diam-diam | Ditambah penanganan error global di `Program.cs` |
| 17 | Nomor nota bentrok bila dua transaksi pada detik yang sama | Ditambahkan milidetik pada format `TRX-yyyyMMddHHmmssfff` |
| 18 | Role tidak dikenal tetap bisa login | Role di luar `Admin` dan `Kasir` ditolak |
| 19 | Jendela yang sama bisa dibuka berkali-kali | Ditambahkan pengecekan form yang sudah terbuka |
| 20 | Angka di tabel tampil apa adanya | Ditambahkan `CellFormatting` dengan format ribuan Indonesia |
| 21 | `SELECT *` bergantung pada urutan kolom | Query ditulis dengan nama kolom eksplisit |

### Berkas Baru

| Berkas | Fungsi |
|---|---|
| `Session.cs` | Menyimpan data user yang sedang login |
| `PasswordHasher.cs` | Hashing dan verifikasi password |
| `InputHelper.cs` | Validasi input dan format nominal |
| `AppConfig.cs` | Penentuan path berkas database dan penyusunan connection string |
| `InputDialog.cs` | Dialog kecil minta satu nilai dari user (ubah jumlah) |
| `appsettings.json` | Konfigurasi path berkas database |
| `skema_database.sql` | Skema MySQL **(sudah dihapus, digantikan `skema.sqlite.sql`)** |
| `migration_restruktur.sql` | Migrasi struktur database ke versi 3.0 **(sudah dihapus)** |
| `migration_id_unsigned.sql` | Migrasi penyamaan tipe kolom ID **(sudah dihapus)** |

### Cacat yang ditemukan oleh Pengujian

Pengujian otomatis menemukan lima bug yang luput dari pemeriksaan manual:

| No | Cacat | Dampak | Perbaikan |
|---|---|---|---|
| 22 | `10.000,123` dibaca sebagai `10.000.123` | Selisih **1000 kali lipat** di nilai transaksi. Kasir mengetik 10.000,123 tetapi sistem menagih 10.000.123. | Input ambigu ditolak dengan pesan yang jelas, bukan ditebak |
| 23 | Subtotal tidak dibulatkan, mis. `3 x 333,33 = 999,99` tapi tersimpan `999,989` | Angka di layar berbeda dengan yang tersimpan, sehingga total nota tidak cocok dengan isi keranjang | `BulatkanSubtotal` memakai `Math.Round` 2 desimal, sama dengan tipe kolom `DECIMAL(15,2)` |
| 24 | `FormatNominal` dipakai untuk kolom stok | Stok `99` tampil sebagai `99,00`, dan `1,5` tampil `1,50` sehingga tidak terbaca sebagai jumlah | Ditambah `FormatJumlah` yang hanya menampilkan pecahan bila memang ada |
| 25 | `satuan` selalu ditulis `pcs` saat menyimpan | Barang satuan `kg` diam-diam berubah jadi `pcs` begitu admin menyuntingnya. Barber timbang jadi salah label. | `satuan` lama dibaca ulang dari database, bukan dari keadaan layar. Berlaku juga saat barang diaktifkan kembali |
| 26 | `is_active` dibaca `TINYINT(1)` jadi `bool` oleh driver, tapi `AmbilDecimal` tidak mengenali `bool` | **Tidak ada user yang bisa login.** `AmbilDecimal(true)` mengembalikan `0`, jadi `Form1` menganggap setiap akun nonaktif. Error "Akun sudah dinonaktifkan" muncul untuk siapa pun, termasuk `admin` | `AmbilDecimal` menangani `bool` dan seluruh tipe integer. Lihat catatan di bawah |

#### Catatan tentang Nomor 26

Ini bug paling merusak dari semua temuan, dan paling mudah lolos dari pemeriksaan
manual, karena kodenya terbaca benar:

```csharp
// Terlihat benar, tapi hasilnya selalu salah:
akunAktif = InputHelper.AmbilDecimal(reader["is_active"]) == 1m;
```

Driver `MySql.Data` mengembalikan kolom `TINYINT(1)` sebagai `System.Boolean`, bukan
sebagai angka. Jadi `reader["is_active"]` berisi `true`, bukan `1`, dan alurnya
menjadi:

```
reader["is_active"]  ->  true   (System.Boolean)
AmbilDecimal(true)   ->  decimal.TryParse("True") gagal  ->  0m
0m == 1m             ->  false  ->  "Akun sudah dinonaktifkan oleh administrator"
```

Aturan yang dipakai mulai sekarang:

1. Nilai `TINYINT(1)` dari database **tidak pernah** dibaca lewat operasi
   aritmetika langsung. Selalu lewat `InputHelper.AmbilDecimal`, yang sekarang
   mengenali `bool`, `sbyte`, `byte`, `short`, `ushort`, `int`, `uint`, `long`,
   `ulong`, `float`, dan `double`.
2. Kalau kolom baru hanya butuh nilai 0 atau 1, lebih baik pakai `SMALLINT`
   supaya tidak ambigu, kecuali memang field itu memang boolean.

Bug ini ditemukan karena pengujian yang sama gagal di dua tempat. Pertama di
harness test, yang melaporkan `is_active` terbaca `0` padahal di database `1`.
Baru setelah itu jejaknya ditelusuri sampai ke kode aplikasi.

### Cacat yang ditemukan saat migrasi ke SQLite

Tujuh cacat berikut tidak mungkin ditemukan selama masih MySQL, karena
penyebabnya adalah perbedaan behavior kedua mesin.

| No | Cacat | Dampak | Perbaikan |
|---|---|---|---|
| 27 | Laporan menjumlahkan `SUM(harga_satuan * qty)` | Total nota di laporan jadi **741,096**, bukan **741,10**. Selisih 3 desimal yang langsung terlihat oleh kasir dan pelanggan. | Laporan menjumlahkan kolom `subtotal` yang sudah dibulatkan, dan setiap agregat dibungkus `ROUND(..., 2)` |
| 28 | Nilai uang di SQLite bisa lebih dari dua desimal | CHECK constraint menolak `1234.567`, jadi transaksi gagal disimpan tanpa pesan jelas | `FormKasir.BulatkanSubtotal` memakai `decimal` C# yang eksak, hasilnya dibulatkan sebelum masuk database |
| 29 | `ExecuteScalar()` setelah `INSERT` mengembalikan `null` | `id_transaksi` bernilai `0`, detail nota tidak pernah terhubung ke header, laporan kosong | `SELECT last_insert_rowid()` pada koneksi yang sama |
| 30 | `Pooling=True` menahan berkas setelah `Close` | `kasir.db` terkunci beberapa detik setelah jendela ditutup, jadi **tidak bisa disalin ke flashdisk** | `Pooling=False` + `Cache=Private` pada connection string |
| 31 | Folder `data` tidak pernah terbentuk | Aplikasi gagal membuka database pada instalasi baru | `AppConfig.GetPathDatabase()` selalu memanggil `Simpan()` yang memanggil `SiapkanFolderInduk()` |
| 32 | Reader SQLite yang masih terbuka menahan kunci | Galat `database is locked` saat satu form masih memegang reader sementara form lain mau menulis | Semua reader dibungkus `using` dalam blok terpisah, ditutup sebelum koneksi lain dibuka |
| 33 | Trigger `updated_at` tanpa `WHEN OLD.x IS NOT NEW.x` | Trigger terpasang, tidak pernah error, tapi juga diam-diam tidak mengubah apa pun. `diperbarui_pada` selalu kosong | Trigger sengaja tanpa syarat `WHEN`, memakai UPDATE bersarang |

Dua jebakan tambahan yang sempat muncul, dan keduanya berhasil dihindari:

- **Trigger dengan `NEW.col = nilai` gagal dimuat.** SQLite tidak mendukung
  penulisan kolom dari trigger dengan cara itu; muncul galat
  `near "NEW": syntax error`. Bentuk resmi yang dipakai adalah UPDATE atau
  INSERT bersarang.
- **PRAGMA `recursive_triggers` harus `OFF`.** Trigger `updated_at`
  melakukan UPDATE ke tabelnya sendiri. Kalau `recursive_triggers` nyala, ia
  memanggil dirinya sendiri berulang kali sampai kehabisan kedalaman.

### Hasil Verifikasi (jaman MySQL)

> **Catatan:** angka di bawah ini tercatat saat aplikasi masih memakai MySQL.
> Setelah migrasi ke SQLite, harness yang sama dijalankan ulang dan sekarang
> totaled **289 lulus, 0 gagal**. Lihat
> [Hasil Pengujian Tahap 1](#hasil-pengujian-tahap-1-sqlite).

Pengujian dilakukan dengan harness terpisah yang meng-compile seluruh kode aplikasi
lalu memanggil method aslinya, termasuk `FormKasir.SimpanTransaksi` yang benar-benar
menulis ke database.

| Area pengujian | Jumlah | Hasil |
|---|---|---|
| `InputHelper.TryParseNominal` | 24 | lulus |
| `InputHelper.FormatNominal` | 17 | lulus |
| `InputHelper.FormatJumlah` (bulat, sesuai satuan pcs) | 11 | lulus |
| `InputHelper.AmbilDecimal` | 8 | lulus |
| `PasswordHasher` | 9 | lulus |
| Koneksi dan versi database | 2 | lulus |
| `SimpanTransaksi` (5 tabel, mutasi stok, snapshot) | 15 | lulus |
| Transaksi pecahan (barang timbang 1,5 kg) | 7 | lulus |
| Rollback saat stok kurang | 5 | lulus |
| Snapshot harga dan nama | 3 | lulus |
| Soft delete barang dan foreign key | 7 | lulus |
| Akun nonaktif dan constraint role | 4 | lulus |
| View laporan | 4 | lulus |
| `BulatkanSubtotal` | 6 | lulus |
| Satuan barang tidak hilang saat edit | 10 | lulus |
| Tipe data dari MySql.Data (`TINYINT(1)` jadi `bool`) | 12 | lulus |
| `InputHelper.TryParseBilanBulat` (tolak "1,5", bukan jadi 15) | 17 | lulus |
| Saringan ketikan kolom angka | 10 | lulus |
| Nominal uang tidak dikoreksi pemformatan | 8 | lulus |
| Qty desimal ditolak, jalur simpan sah tetap benar | 7 | lulus |
| **Total** | **186** | **0 gagal** |

Verifikasi lain yang dilakukan:

- **12 uji constraint SQL** langsung ke database dengan `ROLLBACK`, untuk memastikan
  stok negatif, harga jual nol, qty nol, user tak dikenal, barang tak dikenal, role
  ngawur, hapus barang terjual, dan nomor nota duplikat semuanya benar-benar ditolak.
- **Instalasi bersih diuji dari nol.** Database `db_uji_skema` dibuat dari
  berkas skema, lalu seluruh pengujian dijalankan terhadapnya dan lulus.
- **Rantai migrasi diuji dari nol.** Database lama dipulihkan dari backup, ketiga
  skrip migrasi dijalankan berurutan tanpa error, dan strukturnya dibandingkan
  otomatis dengan instalasi bersih di 89 titik (kolom, index, foreign key, check
  constraint, engine, view). **Tidak ada perbedaan.** Lihat bagian
  [Bukti bahwa Migrasi Benar](#bukti-bahwa-migrasi-benar).
- **Build: 0 error, 0 warning.**
- **Smoke test GUI.** Aplikasi dijalankan sungguhan, jendela login muncul dengan
  judul "Login - Kasir SMK N 4", lalu ditutup tanpa error.
- **Data asli utuh.** Setelah semua pengujian, `db_kasir_smk4` berisi 1 user,
  2 barang, 1 transaksi, 1 detail, 1 mutasi — sama seperti sebelum pengujian.

### Hasil Pengujian Tahap 1 (SQLite)

> **Status: lulus. 289 dari 289, 0 gagal.** Angka ini menggantikan hitungan
> 186 tes di atas, yang masih mencatat hasil jaman MySQL.

Pengujian yang sama dipindahkan ke SQLite, lalu ditambah 6 kelompok baru
khusus untuk memastikan tidak ada yang tertinggal atau berubah diam-diam.

| Kelompok | Isi pengujian |
|---|---|
| A | Pemasangan skema dari nol, baik `skema.sqlite.sql` maupun lewat `Database.PastikanTerpasang` |
| B | Pemecah statement SQL, 12 kasus: titik koma dalam string, `--` dalam string, kutip berulang, badan trigger, dan lain-lain |
| C | Batasan CHECK, FK, UNIQUE, dan `ROUND(x,2) = x` benar-benar menolak |
| D | PRAGMA: `foreign_keys`, `recursive_triggers`, `busy_timeout` |
| E | Trigger `trg_barang_updated` benar-benar mengubah `diperbarui_pada` |
| F | Penentuan path dan penyusunan connection string |

| Konfigurasi | Nilai |
|---|---|
| Jumlah pengujian | 289 |
| Lulus | 289 |
| Gagal | 0 |
| Build | 0 error, 0 warning |
| Database uji | `data\kasir_uji.db` (terpisah dari `kasir.db` milik pengguna) |

Bukti tambahan:

- **Skema identik dengan MySQL, bukan dikira-kira.** Perbandingan otomatis
  terhadap skema MySQL lama: 45 dari 45 kolom cocok, 10 dari 10 index cocok,
  2 dari 2 view cocok, 5 dari 5 foreign key cocok. Selisih objek antara
  `skema.sqlite.sql` dan isi database setelah dipasang: **0**.
- **Harness idempoten.** Dijalankan dua kali, keluarannya 289 baris dan identik
  persis. Setelah selesai, `kasir_uji.db` hanya berisi satu akun `admin`; 4
  tabel lain kosong. Tidak ada sisa pengujian.
- **Tidak ada berkas samping.** Setelah GUI ditutup, folder `data` berisi
  `kasir.db` saja. Tidak ada `kasir.db-wal` maupun `kasir.db-shm`. Berkas
  langsung bisa disalin.
- **Toleransi float diukur, bukan diasumsikan.** `SUM(harga_satuan * qty)`
  untuk 3 baris bernilai `741,096`; total nota sebenarnya `741,10`. Inilah
  alasan laporan menjumlahkan kolom `subtotal`.
- **Bug startup nyata ditemukan oleh pengujian.** Folder `data` ternyata tidak
  pernah terbentuk, karena pembuatan folder hanya ada di jalur yang tidak pernah
  terpakai. Diperbaiki di `AppConfig.GetPathDatabase()`.

### Catatan Penting untuk Data Lama (sudah tidak berlaku)

> **Semua catatan di bawah ini kedaluwarsa.** Data MySQL lama **tidak**
> dimigrasi ke SQLite, jadi tidak ada lagi data lama yang perlu dimigrasi.
> Dump `mysqldump` yang dulu ada di folder `backup/` juga sudah dihapus.
> Bagian ini dipertahankan hanya sebagai catatan apa yang pernah dikerjakan.

1. **Password lama tidak perlu diubah manual.** Password plain text yang sudah ada tetap bisa login, lalu otomatis diubah menjadi hash.
2. **Backup database wajib dibuat** sebelum menjalankan migrasi. Salinan pra-migrasi sudah dihapus bersama skrip migrasinya.
3. **Kolom `qty` dan `stok` berubah tipe** dari `INT` menjadi dua desimal. Nilai lama otomatis jadi `99.00`, tidak ada yang hilang.
4. **`id_transaksi` untuk transaksi lama** diisi otomatis dari `no_nota` masing-masing.
5. **`harga_satuan` untuk transaksi lama** dihitung dari `subtotal / qty`, yaitu harga yang benar-benar dibayar.
6. **Data transaksi lama masuk ke `tb_mutasi_stok`** dengan keterangan "Rekonstruksi transaksi lama", supaya riwayat stok tidak kosong.

### Batasan yang Masih Ada

Fitur berikut memang belum ada dan tidak ikut dikerjakan:

1. Belum ada fitur **cetak struk**.
2. **Layar laporan penjualan sudah ada** (`FormLaporan`): filter periode dan kasir,
   rekap nota, rincian item per nota, produk terlaris, dan ekspor CSV. Yang belum ada
   adalah laporan **laba** dan laporan **stok**.
3. Belum ada **manajemen user** dari aplikasi (tambah user, ubah role, reset password).
   Untuk saat ini, akun baru harus dibuat lewat SQL.
4. Belum ada **riwayat transaksi** dengan fitur cetak ulang nota.
5. Pencarian barang pada mesin kasir hanya berdasarkan barcode, tanpa pencarian berdasarkan nama.
6. Harga jual masih diisi manual, belum ada hitung untung otomatis dari harga beli.
   Data yang dibutuhkan untuk itu sudah ada: `harga_beli`, `harga_jual`, dan
   `harga_satuan` di tabel detail. Untuk laporan laba per nota, `tb_detail_transaksi`
   perlu tambahan kolom snapshot `harga_beli_satuan`.
7. Fitur harga grosir (`minimal_grosir` dan `harga_grosir`) sudah ada di tabel
   `tb_barang` tetapi belum dipakai di antarmuka.
8. Kolom `satuan` **sudah bisa diisi** dari `FormBarang` lewat `cmbSatuan`. Qty dan
   stok sengaja hanya menerima bilangan bulat per satuan (`pcs`).
9. Kolom `diskon_total` dan `diskon_item` sudah ada di database tapi belum dipakai
   di antarmuka. Perhitungan diskon belum ada.
10. Kolom `catatan` di `tb_transaksi` sudah ada tapi belum dipakai.
11. **Tidak ada pembatalan transaksi (void).** Untuk membatalkan, harus lewat SQL.
12. Kolom `id_transaksi` dipakai sebagai foreign key, tapi nomor nota tetap
    disimpan di `tb_detail_transaksi` juga. Sedikit denormalisasi, ini disengaja
    supaya pencarian per nota tidak perlu JOIN.
13. **BUTUH PENGECEKAN ULANG.** Catatan ini ditulis saat aplikasi masih memakai
    MySQL. Sekarang sudah diselesaikan: data berada di satu berkas
    `data\kasir.db`, aplikasi jalan tanpa server, dan foldernya bisa disalin ke
    flashdisk. Lihat [Tahap 1: Dari MySQL ke SQLite](#tahap-1-dari-mysql-ke-sqlite).

### Catatan Teknis untuk Pengembang

- **Menambah role baru** berarti ubah dua tempat: constraint `chk_user_role` di
  database, dan method `SessionIsRoleValid` di `Form1.cs`.
- **Menambah kolom `is_active`** ke tabel baru berarti tambahkan juga filter
  `WHERE is_active = 1` di setiap query yang membaca tabel tersebut.
- **Setiap perubahan stok harus menulis ke `tb_mutasi_stok`.** Kalau ada fitur baru
  yang mengubah stok (mis. fitur retur), jangan lupa tambahkan `CatatMutasiStok`.
- **`BeginTransaction(deferred: false)` di `SimpanTransaksi` tidak boleh diubah
  jadi `BeginTransaction()` biasa.** Yang tanpa `deferred` berarti `BEGIN
  IMMEDIATE`, yaitu kunci tulis diambil sejak transaksi dimulai, bukan saat
  perintah pertama dieksekusi. Inilah pengganti `SELECT ... FOR UPDATE` yang
  dulu dipakai, dan itu yang mencegah dua kasir menjual stok yang sama.
- **Jangan pernah mem-format ulang nominal saat pengguna mengetik.** Fungsi
  `FormatRibuanOtomatis` pernah dipasang karena ia membuang semua karakter
  non-angka lalu menulis ulang sebagai bilangan bulat, sehingga `5000,50` tampil
  `50.050` (100 kali lipat) dan `10000,50` jadi `100.050` sehingga kembalian yang
  ditampilkan ikut salah. Fungsi itu sudah dihapus. Kolom nominal dibiarkan apa
  adanya dan divalidasi saat disimpan dengan `InputHelper.TryParseNominal`.
- **Kolom stok dan qty dikunci ke bilangan bulat** lewat
  `InputHelper.BolehMasukAngka` pada event `KeyPress`. Jangan pakai
  `TryParseNominal` untuk kolom itu: `TryParseNominal("1,5")` menghasilkan `1,5`
  dan `1,5` tidak boleh menjadi `1` atau `2` secara diam-diam. Gunakan
  `InputHelper.TryParseBilanBulat`, yang menolak `1,5` dengan pesan jelas.
- **Pembulatan `FormatJumlah` bukan cara mem-parse.** Nilai sudah divalidasi
  lebih dulu oleh `TryParseBilanBulat`, jadi `FormatJumlah` hanya jadi jaring
  pengaman untuk data lama atau hasil perhitungan.
- **Semua akses database wajib lewat `Koneksi.GetConn()`.** PRAGMA
  `foreign_keys` menempel pada koneksi, bukan pada berkas. Koneksi yang dibuat
  langsung dengan `new SqliteConnection(...)` tidak punya batasan foreign key
  sama sekali, dan pengujian tidak akan menangkapnya.
- **Semua agregat uang wajib dibungkus `ROUND(..., 2)`.** SQLite menyimpan
  `NUMERIC` sebagai pecahan biner, jadi `SUM(harga * qty)` bisa menghasilkan
  `741,096`. Laporan menjumlahkan kolom `subtotal` yang sudah dibulatkan, bukan
  mengalikan ulang.
- **Nilai uang baru boleh disimpan kalau sudah dua desimal.** CHECK constraint
  `ROUND(x, 2) = x` menolak `1234.567` dengan galat
  `CHECK constraint failed`. Jadi `1235.16m * 0.1m` harus dilewatkan
  `FormKasir.BulatkanSubtotal` lebih dulu, bukan langsung di-`INSERT`.
- **Jangan pakai tipe `ENUM`, jangan pakai `AUTO_INCREMENT`.** SQLite hanya
  mengenal `AUTOINCREMENT`. Untuk daftar nilai terbatas, pakai CHECK constraint.
- **`NEW.col = nilai` tidak didukung SQLite.** Untuk menulis kolom dari trigger,
  pakai UPDATE atau INSERT bersarang, seperti yang dilakukan
  `trg_barang_updated`.
- **Jangan pakai `DELETE t FROM t JOIN ...`.** SQLite tidak punya hapus
  multi-tabel. Bentuk yang benar: `DELETE FROM t WHERE id IN (SELECT ... FROM
  x JOIN ...)`.
- **Jangan pakai `journal_mode=WAL`.** Aplikasi ini dirancang untuk flashdisk
  FAT32, dan FAT32 tidak mendukung file journal.
- **Jangan pernah mengatur `Pooling=True`.** Berkas `kasir.db` akan tetap
  terkunci setelah aplikasi ditutup, dan tidak bisa disalin ke flashdisk.

---

## Ide Pengembangan

Sudah selesai:

- [x] Ganti penyimpanan password dengan hashing (`PBKDF2`).
- [x] Pindahkan connection string ke `appsettings.json`.
- [x] Implementasikan `Session.UserId` global dan gunakan pada penyimpanan transaksi.
- [x] Gunakan `transaction` database untuk atomicity saat menyimpan transaksi.
- [x] Validasi stok sebelum scan dan saat pembayaran.
- [x] Dukungan input nominal dengan pemisah ribuan, misalnya `25.000`.
- [x] Terapkan tipe `decimal` untuk seluruh perhitungan nominal di sisi aplikasi.
- [x] Foreign key, index, dan CHECK constraint di database.
- [x] Simpan `uang_diterima` dan `kembalian` supaya rekonsiliasi kas bisa dilakukan.
- [x] Snapshot nama barang dan harga satuan di tabel detail.
- [x] Soft delete barang dan user.
- [x] Ledger `tb_mutasi_stok` untuk audit perubahan stok.
- [x] Jual barang timbang (`qty` dan `stok` jadi `DECIMAL(15,2)`).
- [x] Menu klik kanan untuk mengoreksi jumlah di keranjang.
- [x] View laporan `v_laporan_penjualan` dan `v_stok_gudang`.
- [x] Layar laporan penjualan dengan filter periode, filter kasir, ekspor CSV,
      dan pengoreksi jumlah di keranjang tanpa scan barcode berulang.
- [x] Kolom input **satuan** (`cmbSatuan`) di `FormBarang`.
- [x] Ekspor laporan ke CSV.
- [x] Pindahkan penyimpanan dari MySQL ke **SQLite** supaya aplikasi benar-benar
      offline dan datanya cukup satu berkas untuk disalin ke flashdisk.
- [x] Installer skema otomatis, jadi tidak ada lagi langkah import SQL manual.
- [x] `PRAGMA foreign_keys = ON` di setiap koneksi, bukan sekali di awal aplikasi.
- [x] `Pooling=False` supaya `kasir.db` tidak terkunci setelah aplikasi ditutup.
- [x] Bulatkan seluruh agregat uang dengan `ROUND(..., 2)`.
- [x] Pemecah statement SQL yang sadar trigger, supaya skema bisa dijalankan
      sebagai satu rangkaian perintah.

Belum dikerjakan:
- [ ] Menu **backup dan restore ke flashdisk**.
- [ ] Tambahkan **cetak struk** (printing thermal 58mm) setelah pembayaran berhasil.
- [ ] Tambah **grafik penjualan** pada dashboard admin.
- [ ] Tambahkan **manajemen user** (tambah user, ubah role, reset password).
- [ ] Tambahkan **riwayat transaksi** dengan fitur cetak ulang nota.
- [ ] Tambahkan pencarian barang berdasarkan nama di mesin kasir.
- [ ] **Pembatalan transaksi (void)** dengan alasan, dan catat di `tb_mutasi_stok`
      sebagai mutasi `MASUK` supaya stok kembali.
- [ ] Kolom input **diskon** di antarmuka.
- [ ] Fitur harga grosir (`minimal_grosir` dan `harga_grosir`).
- [ ] Laporan **laba** dari selisih harga jual dan harga beli. Butuh kolom snapshot
      `harga_beli_satuan` di `tb_detail_transaksi`, belum ada.
- [ ] Laporan **stok** (nilai persediaan, barang lambat-moving).
- [ ] Cetak label rak dan barcode dari `FormBarang`.
- [ ] Sinkronisasi stok dengan timbangan digital secara real time.

---

## Lisensi

Proyek ini dibuat untuk keperluan pembelajaran dan tugas sekolah di **SMK Negeri 4**.

---

**Dibuat dengan C# - Windows Forms - .NET 10 - SQLite**
