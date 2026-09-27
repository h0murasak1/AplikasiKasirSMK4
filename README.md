# 🏪 Aplikasi Kasir POS & Manajemen — SMK Negeri 4 Kabupaten Tangerang

![Versi](https://img.shields.io/badge/Versi-1.0.0-blue.svg)
![Platform](https://img.shields.io/badge/Platform-Windows%20Forms%20(WinForms)-0078D7.svg)
![Framework](https://img.shields.io/badge/.NET-10.0%20(Windows%20x64)-512BD4.svg)
![Database](https://img.shields.io/badge/Database-SQLite%20(Offline%20Serverless)-003B57.svg)
![Printer](https://img.shields.io/badge/Hardware-Epson%20TM--U220D%20%2B%20Cash%20Drawer-107C41.svg)

Aplikasi Point of Sale (POS) dan Sistem Manajemen Kasir modern berbasis desktop **Windows Forms (C#)** yang dirancang khusus untuk kebutuhan operasional transaksi dan pengelolaan inventaris di **SMK Negeri 4 Kabupaten Tangerang**.

---

## 📑 Daftar Isi

- [Keunggulan Utama](#-keunggulan-utama)
- [Modul & Fitur Lengkap](#-modul--fitur-lengkap)
- [Desain Antarmuka & Navigasi](#-desain-antarmuka--navigasi)
- [Integrasi Printer Struk & Cash Drawer](#-integrasi-printer-struk--cash-drawer)
- [Cara Instalasi & Menjalankan](#-cara-instalasi--menjalankan)
- [Akun Login Default](#-akun-login-default)
- [Arsitektur Teknis & Database](#-arsitektur-teknis--database)
- [Struktur Direktori Proyek](#-struktur-direktori-proyek)
- [Pencadangan & Pemulihan Data](#-pencadangan--pemulihan-data)
- [Kompatibilitas Sistem Operasi](#-kompatibilitas-sistem-operasi)
- [Lisensi & Hak Cipta](#-lisensi--hak-cipta)

---

## 🚀 Keunggulan Utama

1. **100% Offline & Mandiri (Zero-Configuration)**:
   - Menggunakan basis data **SQLite 3 (`data/kasir.db`)**.
   - Tidak memerlukan server database eksternal (tanpa XAMPP, MySQL Server, Apache, atau internet).
   - Skema database otomatis terpasang dan termigrasi saat aplikasi pertama kali dibuka.
2. **Single-Window Dashboard Navigation**:
   - Desain tata letak terpadu di mana semua menu (Barang, Laporan, Stok, Piutang, Komisi, Master, Backup) terbuka mulus di panel konten utama tanpa membuka jendela baru yang bertumpuk.
3. **Hardware Ready (ESC/POS Printing & Drawer Trigger)**:
   - Terintegrasi langsung dengan printer kasir dot-matrix/thermal (**Epson TM-U220D**, Epson TM-T82, dll.) melalui Windows Spooler RAW printing.
   - Otomatis membuka laci kasir (*Cash Drawer*) via sinyal pulsa RJ-11 dan mencetak struk belanja saat pembayaran sukses.
4. **Siap Pakai & Portable**:
   - Tersedia berkas **`Setup.exe` (Windows Installer Wizard)** dan versi **Portable Folder / ZIP** yang dapat dijalankan langsung dari flashdisk di komputer mana pun.

---

## 📦 Modul & Fitur Lengkap

### 1. 🛒 Mesin Kasir & Transaksi (POS)
- **Pemindaian Barcode Cepat**: Mendukung Barcode Scanner USB (Auto-Enter).
- **Keranjang Belanja Dinamis**: Penambahan otomatis kuantitas bila barcode yang sama di-scan berulang.
- **Harga Grosir Bertingkat**: Otomatis menerapkan harga grosir jika jumlah belanja memenuhi `minimal_grosir`.
- **Integrasi Member & Diskon**: Pilihan pelanggan member, perhitungan poin belanja, dan diskon nota/item manual.
- **Pencatatan Sales & Komisi**: Memilih sales/tenaga penjual untuk perhitungan bagi hasil otomatis.
- **Multi-Metode Pembayaran**: Tunai (Cash), Transfer Bank, QRIS, dan Tempo/Piutang.
- **Keamanan Transaksi Database**: Penyimpanan atomik 4 tabel (`tb_transaksi`, `tb_detail_transaksi`, `tb_barang`, `tb_mutasi_stok`) dengan `BEGIN IMMEDIATE` transaksi guna mencegah selisih stok.

### 2. 📦 Manajemen Master Barang
- **Operasi CRUD Lengkap**: Tambah, ubah, nonaktifkan (*Soft Delete*), dan aktifkan kembali data barang.
- **Data Komprehensif**: Kode barcode unik, nama barang, satuan (`pcs`, `kg`, `box`, `lusin`, dll.), harga beli, harga jual, stok gudang, dan harga grosir.
- **Pencegahan Data Duplikat**: Validasi unik barcode dan sanitasi input angka Indonesia (`1.250.000` atau desimal `0,5`).

### 3. 👥 Manajemen Pengguna & Hak Akses
- **Role Berjenjang**:
  - **Admin**: Akses penuh ke seluruh menu dashboard, laporan, pengaturan master, stok opname, dan backup.
  - **Kasir**: Langsung masuk ke modul kasir dengan antarmuka khusus transaksi.
- **Keamanan Sandi Terenkripsi**: Password di-hash menggunakan algoritma standar industri **PBKDF2-SHA256 (100.000 iterasi + salt acak)**.

### 4. 💳 Piutang & Pembayaran Tempo
- Pencatatan nota belanja tempo/kredit per member/pelanggan.
- Monitoring tanggal jatuh tempo dan status pelunasan piutang.
- Modul pencatatan cicilan dan pelunasan piutang langsung.

### 5. 👔 Komisi Sales & Mitra
- Perhitungan komisi persentase otomatis dari total nota yang ditangani tenaga sales.
- Laporan rekap pencairan komisi per periode.

### 6. 📋 Manajemen Stok & Opname Fisik
- **Kartu Stok & Audit Trail**: Setiap perubahan stok dicatat di tabel `tb_mutasi_stok` (tipe `MASUK`, `KELUAR`, `PENYESUAIAN`).
- **Fitur Stok Opname**: Penyesuaian stok sistem dengan perhitungan fisik riil di toko/gudang.

### 7. 📊 Laporan & Analitik Penjualan
- Filter fleksibel berdasarkan rentang tanggal, kasir, dan metode pembayaran.
- Rekapitulasi omzet penjualan, total laba kotor, dan laba bersih.
- Peringkat produk terlaris (*Top Selling Items*).
- Rincian item per nota transaksi.
- Ekspor seluruh laporan ke format **CSV / Excel**.

### 8. 💾 Cadangan & Pemulihan (Backup & Restore)
- Fitur backup instan database ke file cadangan `.db` langsung dari antarmuka aplikasi.
- Fitur restore data untuk pemulihan darurat saat terjadi kendala hardware.

---

## 🎨 Desain Antarmuka & Navigasi

Aplikasi menggunakan tema modern **Slate Navy Minimalist**:
- **Palet Warna**:
  - Background Utama: `Canvas Slate (#F8FAFC)`
  - Header & Brand: `Navy Dark (#0F172A)`
  - Navigation Bar: `Navy Slate (#1E293B)`
  - Aksen Utama: `Royal Blue (#2563EB)`, `Emerald Green (#10B981)`, `Rose Red (#EF4444)`
- **Tipografi**: Segoe UI dengan tata letak kontras tinggi yang ramah mata (*User Friendly*).
- **Tabel Data**: Zebra striping, tinggi baris optimal, pemformatan nominal mata uang Rupiah otomatis, dan proteksi layout.

---

## 🖨️ Integrasi Printer Struk & Cash Drawer

Aplikasi menggunakan modul native Windows Spooler (`RawPrinterHelper`) untuk mengirimkan instruksi **ESC/POS** langsung ke printer:

```
[Komputer Kasir]
       │ (USB / Virtual Port)
       ▼
[Printer Epson TM-U220D / POS Thermal]
       │ (Kabel RJ-11 / Cash Drawer Kick-Out)
       ▼
[Laci Kasir / Cash Drawer]
```

- **Perintah Laci Kasir**: Mengirimkan sinyal standar ESC/POS `0x1B, 0x70, 0x00, 0x19, 0xFA` ke pin RJ-11 pin 2 & pin 5 sebelum/sesudah mencetak.
- **Format Struk**: 40 kolom teks rapi dengan header profil toko, rincian barang, diskon, total, uang tunai, kembalian, dan footer terima kasih.
- **Menu Pengaturan Printer**: Pengguna dapat memilih printer default, melakukan **Tes Cetak Contoh**, dan melakukan **Tes Buka Laci Kasir**.

---

## 💻 Cara Instalasi & Menjalankan

### Opsi 1: Menggunakan Windows Setup Installer (Paling Direkomendasikan)
1. Unduh atau salin berkas installer:
   📂 **`publish\Setup_KasirSMK4_v1.0.0.exe`**
2. Klik 2x file installer untuk menjalankan Wizard instalasi Windows.
3. Ikuti langkah di layar (shortcut Desktop dan Start Menu akan otomatis dibuat dengan logo resmi SMK Negeri 4).
4. Klik **Finish** untuk langsung menggunakan aplikasi.

### Opsi 2: Versi Portable (Tanpa Instalasi)
1. Salin folder **`publish\KasirSMK4\`** atau ekstrak **`publish\AplikasiKasirSMK4_v1.0.0_SiapPakai.zip`** ke Flashdisk atau drive PC (misal `C:\KasirSMK4`).
2. Buka folder tersebut dan klik 2x pada **`AplikasiKasirSMK4.exe`** atau **`Jalankan_Kasir.bat`**.
3. (Opsional) Klik 2x **`Buat_Shortcut_Desktop.bat`** untuk membuat shortcut di layar desktop.

### Opsi 3: Menjalankan dari Source Code (Developer)
```powershell
# Restore dependensi
dotnet restore

# Build aplikasi
dotnet build

# Jalankan aplikasi
dotnet run
```

---

## 🔑 Akun Login Default

Saat pertama kali dijalankan, sistem secara otomatis menginisialisasi akun administrator:

| Username | Password | Role | Hak Akses |
|---|---|---|---|
| `admin` | `admin123` | `Admin` | Akses penuh ke seluruh menu dan modul sistem |

> ⚠️ **Penting**: Disarankan untuk mengganti kata sandi atau menambahkan akun kasir baru melalui menu **Master -> User** sebelum aplikasi dipakai untuk operasional toko harian.

---

## 🏗️ Arsitektur Teknis & Database

### Stack Teknologi
- **Bahasa Pemrograman**: C# (.NET 10.0 Windows Forms)
- **Target Runtime**: `net10.0-windows` (64-bit Native Single-File Executable)
- **Database Driver**: `Microsoft.Data.Sqlite` v10.0.12 (Engine SQLite 3.53+)
- **UI Library**: `ReaLTaiizor` v3.8.2.1
- **Printer Driver**: Win32 RAW Spooler API (ESC/POS Direct Byte Stream)

### Konfigurasi Database (`appsettings.json`)
```json
{
  "Database": {
    "Path": "data\\kasir.db"
  }
}
```

### PRAGMA Wajib per Koneksi
Setiap koneksi database dikelola melalui `Koneksi.GetConn()` yang mengaktifkan:
- `PRAGMA foreign_keys = ON;` — Memastikan integritas relasi tabel.
- `PRAGMA recursive_triggers = OFF;` — Mencegah pemanggilan trigger berulang.
- `PRAGMA busy_timeout = 5000;` — Menangani antrean transaksi jika disk lambat.

---

## 📁 Struktur Direktori Proyek

```
KASIR_SMK4/
├── AplikasiKasirSMK4.csproj        # Konfigurasi Project .NET
├── AplikasiKasirSMK4.slnx          # Solution File
├── appsettings.json                # Konfigurasi Path Database
├── skema.sqlite.sql                # Skema Database Dasar SQLite
├── KasirSMK4_Installer.iss         # Skrip Kompilasi Inno Setup
│
├── Resources/                      # Aset Logo & Icon Resmi
│   ├── app.ico                     # Multi-Resolution Win32 Icon (32-bit ARGB)
│   ├── app_logo.png                # Logo Kasir SMKN 4 Transparan
│   ├── logo_banner.png             # Logo Banner SMKN 4 Transparan
│   └── logo_smk.png                # Logo Segel SMKN 4 Transparan
│
├── Core & Helpers/
│   ├── Program.cs                  # Entry Point & Global Exception Handler
│   ├── Koneksi.cs                  # Manajemen Koneksi & PRAGMA SQLite
│   ├── Database.cs                 # Skema Installer & Auto-Migration
│   ├── AppConfig.cs                # Resolusi Path Berkas Database
│   ├── Session.cs                  # Manajemen Sesi Pengguna Aktif
│   ├── PasswordHasher.cs           # PBKDF2-SHA256 Password Cryptography
│   ├── InputHelper.cs              # Validasi & Formatter Nominal/Jumlah
│   ├── UiThemeHelper.cs            # Manajemen Tema Warna, Tabel & Icon
│   ├── RawPrinterHelper.cs         # Win32 P/Invoke Direct Printer Spooler
│   └── PosPrinterService.cs        # ESC/POS Receipt Formatter & Cash Drawer
│
├── Forms (Antarmuka Pengguna)/
│   ├── Form1.cs                    # Form Login Pengguna
│   ├── FormMenu.cs                 # Single-Window Dashboard Utama
│   ├── FormKasir.cs                # Form Kasir POS & Transaksi
│   ├── FormBarang.cs               # Form Master Kelola Data Barang
│   ├── FormLaporan.cs              # Form Laporan Penjualan & Ekspor
│   ├── FormStok.cs                 # Form Stok Opname & Mutasi
│   ├── FormPiutang.cs              # Form Manajemen Piutang & Tempo
│   ├── FormKomisi.cs               # Form Perhitungan Komisi Sales
│   ├── FormMember.cs               # Form Master Member Pelanggan
│   ├── FormMasterMenu.cs           # Menu Tile Master Data
│   ├── FormBackup.cs               # Form Backup & Restore Database
│   ├── FormPengaturanPrinter.cs    # Dialog Pemilihan & Tes Printer POS
│   └── FormProfilToko.cs           # Form Pengaturan Profil & Struk
│
└── publish/                        # Hasil Paket Distribusi Siap Pakai
    ├── Setup_KasirSMK4_v1.0.0.exe  # Windows Setup Installer Resmi
    ├── AplikasiKasirSMK4_v1.0.0_SiapPakai.zip # Arsip ZIP Portable
    └── KasirSMK4/                  # Folder Portable Standalone
```

---

## 💾 Pencadangan & Pemulihan Data

1. **Membuat Cadangan Database**:
   - Buka menu **BACKUP** di dashboard aplikasi, lalu klik tombol **Buat Cadangan Sekarang**.
   - Atau salin berkas **`data\kasir.db`** ke media penyimpanan aman (Flashdisk / Cloud).
2. **Memulihkan Database**:
   - Buka menu **BACKUP** -> Pilih file `.db` cadangan -> Klik **Pulihkan Database**.
   - Atau timpa berkas `data\kasir.db` saat aplikasi sedang dalam keadaan tertutup.

---

## 🖥️ Kompatibilitas Sistem Operasi

| Sistem Operasi | Kompatibilitas | Keterangan |
|---|---|---|
| **Windows 11 (64-bit)** |  **100% Kompatibel** | Rekomendasi utama |
| **Windows 10 (64-bit)** |  **100% Kompatibel** | Rekomendasi utama |
| **Windows 8 / 8.1 (64-bit)** |  **Kompatibel** | Langsung jalan |
| **Windows 7 SP1 (64-bit)** | ⚠️ **Kompatibel (Ada Syarat)** | Memerlukan Windows 7 SP1 x64 + update KB2999226 (Universal C Runtime) |

---

## 📜 Lisensi & Hak Cipta

Hak Cipta © 2024 **SMK Negeri 4 Kabupaten Tangerang**.  
Dikembangkan untuk sistem kasir offline mandiri di lingkungan sekolah dan unit produksi SMK Negeri 4.
