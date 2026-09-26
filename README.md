# Aplikasi Kasir SMK Negeri 4

Aplikasi Point of Sale (POS) / mesin kasir berbasis **Windows Forms (C#)** untuk kebutuhan penjualan di **SMK Negeri 4**. Aplikasi ini menangani dua alur utama: **manajemen data barang (gudang)** oleh admin dan **transaksi penjualan dengan pemindaian barcode** oleh kasir.

---

## Daftar Isi

- [Tentang Aplikasi](#tentang-aplikasi)
- [Fitur](#fitur)
- [Teknologi](#teknologi)
- [Prasyarat Instalasi](#prasyarat-instalasi)
- [Setup Database](#setup-database)
- [Konfigurasi Koneksi](#konfigurasi-koneksi)
- [Cara Menjalankan](#cara-menjalankan)
- [Akun Default](#akun-default)
- [Alur Penggunaan](#alur-penggunaan)
- [Struktur Proyek](#struktur-proyek)
- [Detail Perubahan Data](#detail-perubahan-data)
- [Ringkasan Perbaikan](#ringkasan-perbaikan)
- [Ide Pengembangan](#ide-pengembangan)
- [Lisensi](#lisensi)

---

## Tentang Aplikasi

Aplikasi kasir ini dibuat sebagai proyek untuk keperluan penjualan barang di lingkungan SMK Negeri 4. Dirancang dengan pendekatan sederhana: satu aplikasi desktop yang terhubung langsung ke database MySQL lokal (XAMPP), tanpa perlu server tambahan.

Empat layar utama yang tersedia:

| Layar | Nama Form | Keterangan |
|---|---|---|
| Login | `Form1` | Autentikasi user dan pembagian hak akses |
| Dashboard Admin | `FormMenu` | Pintu masuk ke kelola barang, buka kasir, dan logout |
| Master Barang | `FormBarang` | CRUD data barang di gudang |
| Mesin Kasir | `FormKasir` | Scan barcode, keranjang belanja, pembayaran |

---

## Fitur

### 1. Login dan Hak Akses

- Validasi username dan password terhadap tabel `tb_user` di database.
- Sapaan otomatis menampilkan nama lengkap user.
- Pembagian role berbasis kolom `role`:
  - **Admin** diarahkan ke `FormMenu` (dashboard admin).
  - **Kasir** langsung diarahkan ke `FormKasir` (mesin kasir).
- Menggunakan **parameterized query** (`@username`, `@password`) untuk mencegah SQL Injection.
- Otomatis mengosongkan field password dan memfokuskan kolom username saat login gagal.

### 2. Dashboard Admin

Tiga tombol aksi:

- **INPUT DATA BARANG** untuk membuka `FormBarang`.
- **BUKA MESIN KASIR** untuk membuka `FormKasir`.
- **LOGOUT (KELUAR)** untuk menutup dashboard dan kembali ke layar login.

### 3. Kelola Data Barang (CRUD)

Lengkap dengan operasi CRUD untuk tabel `tb_barang`:

- **Create (Simpan)**: menyimpan barang baru dengan kode barcode, nama, harga beli, harga jual, dan stok. Harga beli bersifat opsional (default `0` jika dikosongkan).
- **Read (Tampil)**: seluruh data barang dimuat ke `DataGridView` saat form dibuka.
- **Update (Perbarui)**: memilih baris di tabel otomatis mengisi form input. Kode barcode dikunci (non-editable) saat mode edit untuk menjaga konsistensi data.
- **Delete (Hapus)**: disertai dialog konfirmasi Yes/No sebelum menghapus.
- **Bersihkan**: mengosongkan seluruh kolom input dan membuka kembali kunci kode barcode.

Validasi: kode, nama, harga jual, dan stok wajib diisi sebelum disimpan.

### 4. Mesin Kasir dan Transaksi

- **Scan barcode**: mendukung barcode scanner USB (berperilaku seperti keyboard). Kode dibaca melalui event `Enter` pada kolom barcode.
- **Pencarian otomatis**: barang dicari berdasarkan `kode_barcode` di database.
- **Keranjang belanja**: kolom Kode, Nama Barang, Harga, Qty, Subtotal.
  - Jika barcode yang sama di-scan berulang, **qty otomatis bertambah** tanpa membuat baris baru.
- **Total belanja**: dihitung otomatis setiap kali keranjang berubah.
- **Kembalian otomatis**: dihitung realtime saat uang bayar diketik.
- **Pemeriksaan pembayaran**: transaksi ditolak jika keranjang kosong atau uang bayar kurang dari total.
- **Nomor nota otomatis**: format `TRX-yyyyMMddHHmmss`, contoh `TRX-20260927143022`.
- **Penyimpanan transaksi ke 3 tabel**:
  1. Header transaksi ke `tb_transaksi` (nomor nota, id user, total bayar).
  2. Rincian item ke `tb_detail_transaksi` (nomor nota, kode barcode, qty, subtotal).
  3. Pengurangan stok otomatis: `UPDATE tb_barang SET stok = stok - qty`.
- **Layar otomatis bersih** setelah transaksi berhasil, siap untuk transaksi berikutnya.

---

## Teknologi

| Komponen | Versi / Keterangan |
|---|---|
| Bahasa | C# |
| Framework | .NET 10 (`net10.0-windows`) |
| UI | Windows Forms (WinForms) |
| Database | MySQL / MariaDB (via XAMPP) |
| Connector | `MySql.Data` 26.7.0 |
| UI Library | `ReaLTaiizor` 3.8.2.1 (tema `HopeButton`) |
| IDE | Visual Studio 2022 / Visual Studio Code |

Tidak ada paket tambahan untuk hashing: password memakai **PBKDF2-SHA256** yang sudah tersedia di .NET.

---

## Prasyarat Instalasi

Sebelum menjalankan aplikasi, pastikan sudah tersedia:

1. **Windows 10/11** (aplikasi ini hanya berjalan di Windows).
2. **[.NET 10 SDK](https://dotnet.microsoft.com/download)** untuk proses build.
3. **[XAMPP](https://www.apachefriends.org/)** untuk menjalankan service MySQL.
4. **MySQL Workbench** (opsional) bila ingin mengelola database secara graphical.
5. **Barcode scanner USB** (opsional) untuk transaksi cepat. Kode barcode juga dapat diketik secara manual.

---

## Setup Database

### 1. Jalankan XAMPP

Buka **XAMPP Control Panel**, lalu klik **Start** pada modul **MySQL**. Modul Apache tidak diperlukan untuk aplikasi ini.

### 2. Buat database

Buka browser ke `http://localhost/phpmyadmin`, lalu jalankan script berikut. Script ini disusun agar sesuai dengan query yang digunakan di kode aplikasi.

```sql
CREATE DATABASE IF NOT EXISTS db_kasir_smk4
  DEFAULT CHARACTER SET utf8mb4
  COLLATE utf8mb4_general_ci;

USE db_kasir_smk4;

-- Tabel User
CREATE TABLE tb_user (
  id_user       INT PRIMARY KEY AUTO_INCREMENT,
  username      VARCHAR(50)  NOT NULL,
  password      VARCHAR(255) NOT NULL,
  nama_lengkap  VARCHAR(100) NOT NULL,
  role          VARCHAR(20)  NOT NULL DEFAULT 'Kasir',
  CONSTRAINT uq_user_username UNIQUE (username)
) ENGINE=InnoDB;

-- Tabel Barang / Gudang
CREATE TABLE tb_barang (
  kode_barcode  VARCHAR(50)  NOT NULL PRIMARY KEY,
  nama_barang   VARCHAR(100) NOT NULL,
  harga_beli    INT NOT NULL DEFAULT 0,
  harga_jual    INT NOT NULL DEFAULT 0,
  stok          INT NOT NULL DEFAULT 0
) ENGINE=InnoDB;

-- Tabel Transaksi (header)
CREATE TABLE tb_transaksi (
  id_transaksi  INT PRIMARY KEY AUTO_INCREMENT,
  no_nota       VARCHAR(30) NOT NULL,
  id_user       INT          NOT NULL,
  total_bayar   INT          NOT NULL DEFAULT 0,
  CONSTRAINT uq_transaksi_nota UNIQUE (no_nota)
) ENGINE=InnoDB;

-- Tabel Detail Transaksi (rincian item)
CREATE TABLE tb_detail_transaksi (
  id_detail     INT PRIMARY KEY AUTO_INCREMENT,
  no_nota       VARCHAR(30) NOT NULL,
  kode_barcode  VARCHAR(50) NOT NULL,
  qty           INT          NOT NULL,
  subtotal      INT          NOT NULL DEFAULT 0
) ENGINE=InnoDB;
```

### 3. Isi data awal

```sql
USE db_kasir_smk4;

-- Akun administrator
INSERT INTO tb_user (username, password, nama_lengkap, role) VALUES
  ('admin', 'admin123', 'Administrator', 'Admin');

-- Akun kasir
INSERT INTO tb_user (username, password, nama_lengkap, role) VALUES
  ('kasir', 'kasir123', 'Kasir Toko', 'Kasir');

-- Contoh barang
INSERT INTO tb_barang (kode_barcode, nama_barang, harga_beli, harga_jual, stok) VALUES
  ('8991002101216', 'Indomie Goreng',   2500, 3000, 100),
  ('8992775001369', 'Susu UHT 250ml',   4000, 5000,  50),
  ('8998866202407', 'Teh Pucuk 350ml',  3500, 4500,  80),
  ('8999999000001', 'Buku Tulis 38mm',  3000, 4500, 120),
  ('8999999000002', 'Penghapus',        1500, 2500,  60);
```

> **Penting:** username, password, dan `role` di atas wajib disesuaikan dengan data yang sudah ada di database Anda. Skrip tersebut hanya contoh data awal.

---

## Konfigurasi Koneksi

Credential database **tidak lagi ditulis di dalam kode C#**. Pengaturannya dibaca dari file
**`appsettings.json`** yang otomatis ikut tersalin ke folder output saat build.

```json
{
  "ConnectionStrings": {
    "MySql": "Server=localhost;Database=db_kasir_smk4;Uid=root;Pwd=;"
  }
}
```

| Bagian | Arti | Nilai standar XAMPP |
|---|---|---|
| `Server` | Alamat server MySQL | `localhost` |
| `Database` | Nama database | `db_kasir_smk4` |
| `Uid` | Username MySQL | `root` |
| `Pwd` | Password MySQL | (kosong) |

Urutan pembacaan konfigurasi:

1. Environment variable `KASIR_SMK4_CONNECTION` (takes precedence, berguna untuk testing).
2. File `appsettings.json` di folder aplikasi.
3. Nilai bawaan `localhost` / `db_kasir_smk4` / `root` / kosong.

Saat aplikasi pertama kali dibuka, `Form1_Load` otomatis menjalankan pengecekan koneksi. Jika
berhasil akan muncul MessageBox "Koneksi ke Database MySQL Berhasil!". Jika gagal, aplikasi tetap
terbuka namun transaksi tidak dapat dilakukan, jadi pastikan MySQL sudah berjalan.

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
2. Pastikan MySQL (XAMPP) sudah berjalan.
3. Tekan **Ctrl + F5** (Start Without Debugging) atau klik tombol Run.

### Opsi B: Command Line (dotnet CLI)

```bash
# Restore paket NuGet
dotnet restore

# Build
dotnet build

# Jalankan
dotnet run
```

### Opsi C: Build ke File EXE

```bash
dotnet publish -c Release -r win-x64 --self-contained false
```

File executable akan tersedia di folder `bin\Release\net10.0-windows\win-x64\`.

---

## Akun Default

| Username | Password | Role | Akses |
|---|---|---|---|
| `admin` | `admin123` | Admin | Dashboard admin: kelola barang, buka kasir, logout |
| `kasir` | `kasir123` | Kasir | Langsung ke mesin kasir |

> Ganti password default ini sebelum aplikasi digunakan di lingkungan produksi.

---

## Alur Penggunaan

```
Buka Aplikasi
     |
     v
+-----------------+
|  Layar LOGIN   |   <-- cek koneksi MySQL otomatis
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
5. Pantau **TOTAL BELANJA** yang terupdate otomatis.
6. Masukkan nominal uang bayar, lalu **KEMBALIAN** terhitung otomatis.
7. Klik **BAYAR & SIMPAN**. Nomor nota dan kembalian akan ditampilkan.
8. Stok di gudang otomatis berkurang. Layar siap untuk transaksi berikutnya.

### Skenario 2: Mengelola Data Barang

1. Login sebagai **Admin**, lalu klik **INPUT DATA BARANG**.
2. Isi form: kode barcode, nama, harga beli (opsional), harga jual, stok. Klik **SIMPAN**.
3. Untuk mengubah data, klik baris di tabel. Data otomatis masuk ke form input dan kode barcode terkunci.
4. Ubah data yang diperlukan, lalu klik **PERBARUI**.
5. Untuk menghapus, pilih baris, klik **HAPUS**, lalu konfirmasi **Ya**.
6. Klik **BERSIHKAN** untuk mengembalikan form ke kondisi awal.

---

## Struktur Proyek

```
KASIR_SMK4/
|-- AplikasiKasirSMK4.slnx          # Solution file
|-- AplikasiKasirSMK4.csproj        # Project file (.NET 10, WinForms)
|-- appsettings.json                # Konfigurasi connection string
|-- README.md                       # Dokumentasi ini
|
|-- Program.cs                      # Entry point + penanganan error global
|-- Koneksi.cs                      # Class koneksi MySQL dan tes koneksi
|-- AppConfig.cs                    # Pembacaan appsettings.json
|-- Session.cs                      # Data user yang sedang login
|-- PasswordHasher.cs               # Hashing password PBKDF2-SHA256
|-- InputHelper.cs                  # Validasi dan format input nominal
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

---

## Detail Perubahan Data

### 1. `Koneksi.cs` - Manajemen Koneksi

- `GetConn()`: mengembalikan objek `MySqlConnection` baru. Dipanggil dengan pola `using` di setiap form agar koneksi otomatis ditutup.
- `TestConnection(out string pesanError)`: mencoba membuka koneksi dan mengembalikan `true` atau `false` tanpa menampilkan MessageBox, sehingga pemanggil yang menentukan cara menampilkannya.
- Connection string dibaca dari `appsettings.json` melalui `AppConfig`.

### 2. `Form1.cs` - Autentikasi

Query yang digunakan:

```sql
SELECT id_user, password, nama_lengkap, role
FROM tb_user WHERE username = @username LIMIT 1;
```

- Password diverifikasi dengan `PasswordHasher.Verify`, yang otomatis menangani password lama (plain text) maupun hash baru.
- Password lama yang berhasil login otomatis di-upgrade menjadi hash.
- Role yang tidak dikenali (`Admin` / `Kasir`) akan ditolak.
- Data user yang berhasil login disimpan di `Session` agar tidak perlu login berulang kali.
- `Session.UserId` inilah yang kini dipakai saat menyimpan transaksi.

### 3. `FormBarang.cs` - CRUD Barang

| Aksi | Method | Perintah SQL |
|---|---|---|
| Tambah | `btnSimpan_Click` | `INSERT INTO tb_barang ...` |
| Tampil | `TampilData()` | `SELECT ... FROM tb_barang ORDER BY nama_barang ASC` |
| Ubah | `btnEdit_Click` | `UPDATE tb_barang SET ... WHERE kode_barcode = @kode` |
| Hapus | `btnHapus_Click` | `DELETE FROM tb_barang WHERE kode_barcode = @kode` |
| Isi form | `dgvBarang_CellClick` | Membaca baris tabel ke kolom input |
| Validasi | `AmbilInput` | Memeriksa kode, nama, harga jual, harga beli, dan stok |
| Reset form | `BersihkanForm()` | Mengosongkan kolom input |

Perlindungan tambahan:

- Kode barcode duplikat dicegah sebelum insert, dengan pesan yang jelas.
- `MySqlException` dengan nomor 1062 (duplicate key) ditangani khusus.
- `MySqlException` dengan nomor 1451 (foreign key constraint) ditangani sebagai "barang sudah dipakai pada transaksi sebelumnya".
- Role non-Admin tidak dapat membuka form ini.
- Harga dan stok divalidasi sebagai angka, tidak negatif, dan stok harus bilangan bulat.

### 4. `FormKasir.cs` - Transaksi

| Fitur | Method | Keterangan |
|---|---|---|
| Scan barcode | `txtBarcode_KeyDown` | Trigger pada tombol `Enter`, beep dicegah |
| Cari barang | `CariDanMasukKeranjang` | Validasi stok, cek duplikat, tambah qty |
| Hapus item | `dgvKeranjang_CellDoubleClick` | Double click baris untuk menghapus item |
| Hitung total | `HitungTotalBelanja` | Menjumlahkan kolom subtotal ke `_totalBelanja` |
| Hitung kembalian | `PerbaruiKembalian` | `kembalian = uangBayar - total`, tanpa nilai negatif |
| Bayar | `btnBayar_Click` | Validasi keranjang, nominal, dan pembayaran cukup |
| Simpan | `SimpanTransaksi` | `BEGIN` / `COMMIT` / `ROLLBACK` |

**Transaksi database.** Seluruh penyimpanan dibungkus `MySqlTransaction`. Bila satu saja query
gagal, semua perubahan dibatalkan dan stok tidak akan berkurang sebagian.

**Penguncian baris.** Sebelum mengurangi stok, aplikasi menjalankan
`SELECT ... FOR UPDATE` pada baris barang. Ini mencegah race condition ketika dua kasir
menjual barang yang sama pada saat bersamaan.

**Validasi stok dua lapis:**

1. Saat scan barcode: stok `0` ditolak, dan total qty di keranjang tidak boleh melebihi stok.
2. Saat pembayaran: stok dicek ulang dari database di dalam transaction.

Urutan query saat pembayaran:

```sql
-- 1. Header transaksi (id_user dari Session, bukan angka hardcode)
INSERT INTO tb_transaksi (no_nota, id_user, total_bayar) VALUES (...);

-- 2. Untuk setiap baris keranjang:
--    a. Cek dan kunci stok
SELECT nama_barang, stok FROM tb_barang WHERE kode_barcode = @kode FOR UPDATE;

--    b. Simpan rincian item
INSERT INTO tb_detail_transaksi (no_nota, kode_barcode, qty, subtotal) VALUES (...);

--    c. Kurangi stok
UPDATE tb_barang SET stok = stok - @qty WHERE kode_barcode = @kode;
```

### 5. `InputHelper.cs` - Validasi Input

Menghemat penulisan kode validasi di setiap form, dan mendukung format input Indonesia:

| Input | Hasil |
|---|---|
| `3000` | 3000 |
| `3.000` atau `30,000` | 3000 (dibaca sebagai pemisah ribuan) |
| `1.250.000` | 1250000 (ribuan bertingkat) |
| `2500.50` atau `2500,50` | 2500,50 (dibaca sebagai desimal) |
| `abc`, `-500`, `Rp 5000` | Ditolak dengan pesan jelas |

Untuk output, `FormatNominal` memakai format Indonesia: `12500` menjadi `12.500` dan
`2500.5` menjadi `2.500,50`.

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

## Catatan dan Batasan Saat Ini

Hal-hal berikut diketahui sebagai limitasi pada versi saat ini dan dapat menjadi bahan perbaikan:

1. **Password disimpan sebagai teks biasa (plain text).** Seharusnya di-hash menggunakan `BCrypt.Net-Next` atau `PBKDF2`.
2. **Kredensial database ditulis langsung di dalam `Koneksi.cs`.** Sebaiknya dipindahkan ke `app.config` atau file terpisah.
3. **Nilai `id_user` pada transaksi masih hardcode `1`.** Seharusnya menggunakan ID user yang sedang login.
4. **Nomor nota berpotensi bentrok** jika dua transaksi dilakukan pada detik yang sama. Pertimbangkan `AUTO_INCREMENT` atau `LastInsertId()`.
5. **Perhitungan nilai uang menggunakan tipe `int`.** Untuk nominal rupiah besar atau nilai pecahan, disarankan `decimal` atau `long`.
6. **Pengurangan stok tidak di dalam transaksi database (`BEGIN` / `COMMIT`).** Berpotensi terjadi race condition saat dua kasir melakukan penjualan bersamaan.
7. **Belum ada validasi stok** sebelum scan. Barang dengan stok `0` masih bisa dimasukkan ke keranjang.
8. **Tidak ada fitur cetak struk** dan **laporan penjualan** (harian, bulanan, atau per barang).
9. **Session user tidak disimpan** secara global, sehingga login ulang diperlukan tiap kali membuka dashboard.
10. **Pencarian barang** pada mesin kasir hanya berdasarkan barcode, tanpa fitur pencarian berdasarkan nama.

---

## Ringkasan Perbaikan

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
| `AppConfig.cs` | Pembacaan `appsettings.json` |
| `appsettings.json` | Konfigurasi connection string |

### Hasil Verifikasi

- Build: **0 error, 0 warning**.
- Fungsi `PasswordHasher` dan `InputHelper` diuji dengan **41 kasus uji**, seluruhnya lulus.
- File di folder `bin\Debug\net10.0-windows\appsettings.json` sudah otomatis ikut tersalin.

### Catatan Penting untuk Data Lama

1. **Password lama tidak perlu diubah manual.** Password plain text yang sudah ada tetap bisa login, lalu otomatis diubah menjadi hash.
2. **Tabel database tidak perlu diubah.** Kode kolom dan strukturnya sama seperti sebelumnya.
3. **Disarankan melakukan backup database** sebelum terutama jika sudah banyak transaksi tersimpan.

### Batasan yang Masih Ada

Fitur berikut memang belum ada dan tidak ikut dikerjakan pada perbaikan ini:

1. Belum ada fitur **cetak struk**.
2. Belum ada **laporan penjualan** harian atau bulanan.
3. Belum ada **manajemen user** (tambah user, ubah role, reset password dari aplikasi).
4. Belum ada **riwayat transaksi** dengan fitur cetak ulang nota.
5. Kolom nominal di database masih `INT`, sehingga harga pecahan (misal `2500,50`) akan dibulatkan oleh MySQL. Untuk mendukung pecahan, jalankan skrip di bawah.
6. Pencarian barang pada mesin kasir hanya berdasarkan barcode, tanpa pencarian berdasarkan nama.
7. Harga jual masih diisi manual, belum ada hitung untung otomatis dari harga beli.

### Migrasi Opsional: Mendukung Harga Pecahan

Hanya jalankan bila memang membutuhkan harga dengan angka desimal:

```sql
USE db_kasir_smk4;

ALTER TABLE tb_barang
  MODIFY harga_beli DECIMAL(15,2) NOT NULL DEFAULT 0.00,
  MODIFY harga_jual DECIMAL(15,2) NOT NULL DEFAULT 0.00,
  MODIFY stok         INT          NOT NULL DEFAULT 0;

ALTER TABLE tb_transaksi
  MODIFY total_bayar DECIMAL(15,2) NOT NULL DEFAULT 0.00;

ALTER TABLE tb_detail_transaksi
  MODIFY subtotal DECIMAL(15,2) NOT NULL DEFAULT 0.00;
```

Kode aplikasi sudah siap mendukung nilai desimal, jadi migrasi ini tidak memerlukan perubahan kode.

---

## Ide Pengembangan

Saran untuk pengembangan lanjutan aplikasi:

- [x] Ganti penyimpanan password dengan hashing (`PBKDF2`).
- [x] Pindahkan connection string ke `appsettings.json`.
- [x] Implementasikan `Session.UserId` global dan gunakan pada penyimpanan transaksi.
- [x] Gunakan `transaction` MySQL untuk atomicity saat menyimpan transaksi.
- [x] Validasi stok sebelum scan dan saat pembayaran.
- [x] Dukungan input nominal dengan pemisah ribuan, misalnya `25.000`.
- [x] Terapkan tipe `decimal` untuk seluruh perhitungan nominal di sisi aplikasi.
- [ ] Tambahkan **cetak struk** (printing thermal 58mm) setelah pembayaran berhasil.
- [ ] Buat **laporan penjualan** harian atau bulanan dengan filter tanggal.
- [ ] Tambah **grafik penjualan** pada dashboard admin.
- [ ] Tambahkan **manajemen user** (tambah user, ubah role, reset password).
- [ ] Tambahkan **riwayat transaksi** dengan fitur cetak ulang nota.
- [ ] Tambahkan pencarian barang berdasarkan nama di mesin kasir.
- [ ] Hitung otomatis laba dari selisih harga jual dan harga beli.
- [ ] Tambahkan hak akses tiga tingkat untuk memisahkan Admin, Kasir, dan Manajer.

---

## Lisensi

Proyek ini dibuat untuk keperluan pembelajaran dan tugas sekolah di **SMK Negeri 4**.

---

**Dibuat dengan C# - Windows Forms - .NET 10 - MySQL**
