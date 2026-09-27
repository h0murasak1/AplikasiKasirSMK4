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
- [Konfigurasi Koneksi](#konfigurasi-koneksi)
- [Cara Menjalankan](#cara-menjalankan)
- [Akun Default](#akun-default)
- [Alur Penggunaan](#alur-penggunaan)
- [Struktur Proyek](#struktur-proyek)
- [Detail Perubahan Data](#detail-perubahan-data)
- [Ringkasan Perbaikan](#ringkasan-perbaikan)
- [Restrukturasi Database](#restrukturasi-database)
- [Ide Pengembangan](#ide-pengembangan)
- [Lisensi](#lisensi)

---

## Tentang Aplikasi

Aplikasi kasir ini dibuat sebagai proyek untuk keperluan penjualan barang di lingkungan SMK Negeri 4. Dirancang dengan pendekatan sederhana: satu aplikasi desktop yang terhubung langsung ke database MySQL lokal (XAMPP), tanpa perlu server tambahan.

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
Stok boleh pecahan karena `stok` sudah bertipe `DECIMAL(15,2)`.

### 4. Mesin Kasir dan Transaksi

- **Scan barcode**: mendukung barcode scanner USB (berperilaku seperti keyboard). Kode dibaca melalui event `Enter` pada kolom barcode.
- **Pencarian otomatis**: barang dicari berdasarkan `kode_barcode` **dan harus berstatus aktif**. Barang yang dinonaktifkan akan ditolak walau barcode-nya masih tersimpan.
- **Keranjang belanja**: kolom Kode, Nama Barang, Harga, Qty, Subtotal.
  - Jika barcode yang sama di-scan berulang, **qty otomatis bertambah** tanpa membuat baris baru.
- **Ubah jumlah dengan klik kanan**: klik kanan pada baris keranjang untuk membuka
  menu **Ubah Jumlah...** atau **Hapus Item**. Menu ubah jumlah menerima pecahan,
  misalnya `0,5` kg, sekaligus memeriksa ulang stok terbaru dari database.
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

### 2. Pasang skema database

Seluruh struktur tabel, constraint, index, dan data contoh sudah tersedia di satu berkas:
**`skema_database.sql`**. Jalankan berkas itu, tidak perlu menulis query sendiri.

**Cara A — phpMyAdmin**

1. Buka `http://localhost/phpmyadmin`.
2. Klik tab **Import** di menu atas.
3. Klik **Choose File**, pilih `skema_database.sql`.
4. Pastikan **Character set of the file** = `utf-8`.
5. Klik **Go** di bagian bawah.

**Cara B — Command Line (PowerShell)**

```powershell
cmd /c "C:\xampp\mysql\bin\mysql.exe -u root --default-character-set=utf8mb4 < skema_database.sql"
```

> PowerShell tidak mendukung operator `<` untuk redirect. Karena itu perintah
> di atas dibungkus dengan `cmd /c`. Kalau `mysql.exe` ada di lokasi lain,
> sesuaikan jalurnya.

**Cara C — MySQL Workbench**

1. Buka Workbench, hubungkan ke `localhost` dengan user `root` tanpa password.
2. Klik **File > Open SQL Script**, pilih `skema_database.sql`.
3. Klik Execute (bolt).

Selesai. Database `db_kasir_smk4`, 5 tabel, 2 view, dan 3 barang contoh sudah siap.

### 3. Kalau database Anda sudah ada (versi lama)

**Jangan** jalankan `skema_database.sql` pada database yang sudah berisi data, karena
skrip tersebut akan gagal. Jalankan migrasi berikut sesuai urutannya:

| Urutan | Berkas | Isi |
|---|---|---|
| 1 | `migration_pecahan.sql` | Nominal `INT` jadi `DECIMAL(15,2)` |
| 2 | `migration_restruktur.sql` | Foreign key, index, check constraint, snapshot, mutasi stok, soft delete, satuan, qty pecahan |
| 3 | `migration_id_unsigned.sql` | Menyamakan tipe kolom ID jadi `UNSIGNED` |

Backup pra-migrasi tersimpan di folder `backup/`. Lihat bagian
[Restrukturasi Database](#restrukturasi-database) untuk penjelasan detailnya.

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
| `admin` | `admin123` | Admin | Dashboard admin: kelola barang, buka kasir, laporan penjualan, logout |
| `kasir` | `kasir123` | Kasir | Langsung diarahkan ke mesin kasir (`FormKasir`) untuk melayani transaksi |

> **Ganti password ini sebelum aplikasi dipakai di lingkungan produksi.**
> Password default di atas berupa teks biasa. Begitu login berhasil pertama kali,
> aplikasi otomatis mengubahnya menjadi hash PBKDF2-SHA256.

Role yang diterima database hanya `Admin` dan `Kasir`. Nilai lain akan ditolak
oleh `chk_user_role`.

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
|-- appsettings.json                # Konfigurasi connection string
|-- skema_database.sql              # SKEMA LENGKAP untuk instalasi baru
|-- migration_pecahan.sql           # Migrasi: nominal INT jadi DECIMAL(15,2)
|-- migration_restruktur.sql        # Migrasi: FK, index, check, snapshot, mutasi
|-- migration_id_unsigned.sql       # Migrasi: samakan tipe kolom ID
|-- README.md                       # Dokumentasi ini
|-- backup/                         # Backup database sebelum migrasi
|
|-- Program.cs                      # Entry point + penanganan error global
|-- Koneksi.cs                      # Class koneksi MySQL dan tes koneksi
|-- AppConfig.cs                    # Pembacaan appsettings.json
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

- `GetConn()`: mengembalikan objek `MySqlConnection` baru. Dipanggil dengan pola `using` di setiap form agar koneksi otomatis ditutup.
- `TestConnection(out string pesanError)`: mencoba membuka koneksi dan mengembalikan `true` atau `false` tanpa menampilkan MessageBox, sehingga pemanggil yang menentukan cara menampilkannya.
- Connection string dibaca dari `appsettings.json` melalui `AppConfig`.

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
- `MySqlException` dengan nomor 1062 (duplicate key) ditangani khusus.
- Role non-Admin tidak dapat membuka form ini.
- Harga dan stok divalidasi sebagai angka, tidak negatif, dan tidak melebihi kapasitas `DECIMAL(15,2)`.
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

**Transaksi database.** Seluruh penyimpanan dibungkus `MySqlTransaction`. Bila satu saja query
gagal, semua perubahan dibatalkan dan stok tidak akan berkurang sebagian.

**Penguncian baris.** Sebelum mengurangi stok, aplikasi menjalankan
`SELECT ... FOR UPDATE` pada baris barang. Ini mencegah race condition ketika dua kasir
menjual barang yang sama pada saat bersamaan.

**Validasi stok tiga lapis:**

1. Saat scan barcode: barang non-aktif ditolak, stok `0` ditolak, dan total qty di keranjang tidak boleh melebihi stok.
2. Saat ubah jumlah dari menu klik kanan: stok terbaru dibaca ulang dari database.
3. Saat pembayaran: stok dicek ulang dari database di dalam transaction.

Urutan query saat pembayaran:

```sql
-- 1. Header transaksi (id_user dari Session, bukan angka hardcode)
INSERT INTO tb_transaksi (no_nota, id_user, total_bayar, uang_diterima, kembalian)
VALUES (@noNota, @idUser, @totalBayar, @uangDiterima, @kembalian);

-- id_transaksi hasil insert diambil lewat LastInsertedId untuk dipakai di bawah

-- 2. Untuk setiap baris keranjang:
--    a. Cek dan kunci stok
SELECT nama_barang, stok FROM tb_barang WHERE kode_barcode = @kode FOR UPDATE;

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

## Restrukturasi Database

> **Status: sudah diterapkan.** Skrip tersedia di `migration_restruktur.sql` dan
> `migration_id_unsigned.sql` bila Anda perlu menjalankannya di instalasi lain.
> Untuk instalasi baru, cukup pakai `skema_database.sql`.

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

### Cara Menjalankan Migrasi

```powershell
# WAJIB: backup dulu
cmd /c "C:\xampp\mysql\bin\mysqldump.exe -u root --default-character-set=utf8mb4 --single-transaction db_kasir_smk4 > backup_sebelum.sql"

# Lalu jalankan berurutan
cmd /c "C:\xampp\mysql\bin\mysql.exe -u root --default-character-set=utf8mb4 < migration_pecahan.sql"
cmd /c "C:\xampp\mysql\bin\mysql.exe -u root --default-character-set=utf8mb4 < migration_restruktur.sql"
cmd /c "C:\xampp\mysql\bin\mysql.exe -u root --default-character-set=utf8mb4 < migration_id_unsigned.sql"
```

Ketiganya aman dijalankan berulang kali dan tidak menghapus data.

> **Penting:** `migration_id_unsigned.sql` harus dijalankan **terakhir** dan setelah
> `migration_restruktur.sql`, karena foreign key tidak boleh diubah selama masih
> terhubung. Skrip itu sudah menangani urutannya sendiri (drop FK → ubah kolom →
> buat ulang FK).

### Cara Restore Backup

Berkas di folder `backup/` sengaja dibuat **tanpa** `CREATE DATABASE` dan `USE`,
supaya bisa dipulihkan ke database dengan nama lain saat diuji. Karena itu
namanya database harus disebutkan di perintah:

```powershell
# Restore ke database yang sudah ada
cmd /c "C:\xampp\mysql\bin\mysql.exe -u root --default-character-set=utf8mb4 db_kasir_smk4 < backup\nama_file_backup.sql"

# Atau buat dulu database-nya, baru restore
cmd /c "C:\xampp\mysql\bin\mysql.exe -u root -e ""CREATE DATABASE db_kasir_smk4 DEFAULT CHARACTER SET utf8mb4"""
cmd /c "C:\xampp\mysql\bin\mysql.exe -u root --default-character-set=utf8mb4 db_kasir_smk4 < backup\nama_file_backup.sql"
```

### Bukti bahwa Migrasi Benar

Rantai migrasi diuji dari nol, bukan diasumsikan benar:

1. Database `db_uji_migrasi` dibuat dengan memulihkan backup struktur lama
   (`backup/db_kasir_smk4_sebelum_restruktur_*.sql`) yang isinya 4 tabel tanpa
   foreign key.
2. Ketiga skrip migrasi dijalankan berurutan. Semuanya keluar dengan kode 0
   dan tanpa satu pun peringatan.
3. Struktur hasilnya dibandingkan otomatis dengan instalasi bersih dari
   `skema_database.sql`, di 89 titik: definisi kolom (tipe, urutan, null,
   default), index, foreign key beserta aturan cascade, check constraint,
   engine, collation, dan daftar view.
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
| `InputDialog.cs` | Dialog kecil minta satu nilai dari user (ubah jumlah) |
| `appsettings.json` | Konfigurasi connection string |
| `skema_database.sql` | Skema lengkap untuk instalasi baru |
| `migration_restruktur.sql` | Migrasi struktur database ke versi 3.0 |
| `migration_id_unsigned.sql` | Migrasi penyamaan tipe kolom ID |

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

### Hasil Verifikasi

Pengujian dilakukan dengan harness terpisah yang meng-compile seluruh kode aplikasi
lalu memanggil method aslinya, termasuk `FormKasir.SimpanTransaksi` yang benar-benar
menulis ke database.

| Area pengujian | Jumlah | Hasil |
|---|---|---|
| `InputHelper.TryParseNominal` | 24 | lulus |
| `InputHelper.FormatNominal` | 17 | lulus |
| `InputHelper.FormatJumlah` | 9 | lulus |
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
| **Total** | **142** | **0 gagal** |

Verifikasi lain yang dilakukan:

- **12 uji constraint SQL** langsung ke database dengan `ROLLBACK`, untuk memastikan
  stok negatif, harga jual nol, qty nol, user tak dikenal, barang tak dikenal, role
  ngawur, hapus barang terjual, dan nomor nota duplikat semuanya benar-benar ditolak.
- **Instalasi bersih diuji dari nol.** Database `db_uji_skema` dibuat dari
  `skema_database.sql`, lalu seluruh pengujian dijalankan terhadapnya dan lulus.
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

### Catatan Penting untuk Data Lama

1. **Password lama tidak perlu diubah manual.** Password plain text yang sudah ada tetap bisa login, lalu otomatis diubah menjadi hash.
2. **Backup database wajib dibuat** sebelum menjalankan migrasi. Salinan pra-migrasi ada di folder `backup/`.
3. **Kolom `qty` dan `stok` berubah tipe** dari `INT` menjadi `DECIMAL(15,2)`. Nilai lama otomatis jadi `99.00`, tidak ada yang hilang.
4. **`id_transaksi` untuk transaksi lama** diisi otomatis dari `no_nota` masing-masing.
5. **`harga_satuan` untuk transaksi lama** dihitung dari `subtotal / qty`, yaitu harga yang benar-benar dibayar.
6. **Data transaksi lama masuk ke `tb_mutasi_stok`** dengan keterangan "Rekonstruksi transaksi lama", supaya riwayat stok tidak kosong.

### Batasan yang Masih Ada

Fitur berikut memang belum ada dan tidak ikut dikerjakan:

1. Belum ada fitur **cetak struk**.
2. Belum ada layar **laporan penjualan** di aplikasi. Query sudah tersedia lewat
   view `v_laporan_penjualan` dan `v_stok_gudang`, tapi belum ada halaman untuk menampilkannya.
3. Belum ada **manajemen user** dari aplikasi (tambah user, ubah role, reset password).
   Untuk saat ini, akun baru harus dibuat lewat SQL.
4. Belum ada **riwayat transaksi** dengan fitur cetak ulang nota.
5. Pencarian barang pada mesin kasir hanya berdasarkan barcode, tanpa pencarian berdasarkan nama.
6. Harga jual masih diisi manual, belum ada hitung untung otomatis dari harga beli.
   Data yang dibutuhkan untuk itu sudah ada: `harga_beli`, `harga_jual`, dan
   `harga_satuan` di tabel detail.
7. Fitur harga grosir (`minimal_grosir` dan `harga_grosir`) sudah ada di tabel
   `tb_barang` tetapi belum dipakai di antarmuka.
8. Kolom `satuan` sudah ada di database dan tampil di tabel, tapi **belum bisa diisi
   dari form**. Untuk barang baru nilainya `pcs`, dan untuk barang lama satuan
   aslinya dipertahankan saat disunting. Mengubahnya perlu lewat SQL, atau dengan
   menambahkan kontrol baru di `FormBarang.Designer.cs`.
9. Kolom `diskon_total` dan `diskon_item` sudah ada di database tapi belum dipakai
   di antarmuka. Perhitungan diskon belum ada.
10. Kolom `catatan` di `tb_transaksi` sudah ada tapi belum dipakai.
11. **Tidak ada pembatalan transaksi (void).** Untuk membatalkan, harus lewat SQL.
12. Kolom `id_transaksi` dipakai sebagai foreign key, tapi nomor nota tetap
    disimpan di `tb_detail_transaksi` juga. Sedikit denormalisasi, ini disengaja
    supaya pencarian per nota tidak perlu JOIN.

### Catatan Teknis untuk Pengembang

- **Menambah role baru** berarti ubah dua tempat: constraint `chk_user_role` di
  database, dan method `SessionIsRoleValid` di `Form1.cs`.
- **Menambah kolom `is_active`** ke tabel baru berarti tambahkan juga filter
  `WHERE is_active = 1` di setiap query yang membaca tabel tersebut.
- **Setiap perubahan stok harus menulis ke `tb_mutasi_stok`.** Kalau ada fitur baru
  yang mengubah stok (mis. fitur retur), jangan lupa tambahkan `CatatMutasiStok`.
- **`SELECT ... FOR UPDATE` di `SimpanTransaksi` tidak boleh dihapus.** Itu yang
  mencegah dua kasir menjual stok yang sama.

---

## Ide Pengembangan

Sudah selesai:

- [x] Ganti penyimpanan password dengan hashing (`PBKDF2`).
- [x] Pindahkan connection string ke `appsettings.json`.
- [x] Implementasikan `Session.UserId` global dan gunakan pada penyimpanan transaksi.
- [x] Gunakan `transaction` MySQL untuk atomicity saat menyimpan transaksi.
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

Belum dikerjakan:

- [ ] Tambahkan **cetak struk** (printing thermal 58mm) setelah pembayaran berhasil.
- [ ] Buat **layar laporan penjualan** harian atau bulanan dengan filter tanggal.
      Query-nya sudah siap, tinggal ditampilkan.
- [ ] Tambah **grafik penjualan** pada dashboard admin.
- [ ] Tambahkan **manajemen user** (tambah user, ubah role, reset password).
- [ ] Tambahkan **riwayat transaksi** dengan fitur cetak ulang nota.
- [ ] Tambahkan pencarian barang berdasarkan nama di mesin kasir.
- [ ] **Pembatalan transaksi (void)** dengan alasan, dan catat di `tb_mutasi_stok`
      sebagai mutasi `MASUK` supaya stok kembali.
- [ ] Kolom input **satuan** dan **diskon** di antarmuka.
- [ ] Fitur harga grosir (`minimal_grosir` dan `harga_grosir`).
- [ ] Hitung otomatis laba dari selisih harga jual dan harga beli. Data yang
      dibutuhkan sudah lengkap di `tb_detail_transaksi.harga_satuan`.
- [ ] Cetak label rak dan barcode dari `FormBarang`.
- [ ] Ekspor laporan ke Excel atau CSV.
- [ ] Sinkronisasi stok dengan timbangan digital secara real time.

---

## Lisensi

Proyek ini dibuat untuk keperluan pembelajaran dan tugas sekolah di **SMK Negeri 4**.

---

**Dibuat dengan C# - Windows Forms - .NET 10 - MySQL**
