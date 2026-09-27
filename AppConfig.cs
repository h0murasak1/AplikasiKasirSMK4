using System.Text.Json;

namespace AplikasiKasirSMK4
{
    /// <summary>
    /// Menentukan letak berkas database kasir.db.
    /// </summary>
    /// <remarks>
    /// Mulai Tahap 1 aplikasi tidak lagi memakai MySQL. Konfigurasi yang
    /// dulunya berupa connection string sekarang berupa PATH BERKAS.
    /// <para>
    /// Path default-nya adalah folder "data" di samping berkas executable.
    /// Pilihan itu disengaja: seluruh aplikasi tinggal disalin ke flashdisk
    /// dan langsung jalan tanpa XAMPP, tanpa layanan database, dan tanpa
    /// setting sama sekali. Folder data yang terpisah dari folder executable
    /// juga membuat berkas kasir.db mudah dikenali saat membuat cadangan.
    /// </para>
    /// <para>
    /// Environment variable KASIR_SMK4_CONNECTION yang dulu dipakai untuk
    /// meng-override connection string sudah tidak berlaku. Yang menggantikannya
    /// adalah KASIR_SMK4_DB yang berisi path ke berkas .db.
    /// </para>
    /// </remarks>
    internal static class AppConfig
    {
        /// <summary>Nama berkas database di dalam folder data.</summary>
        public const string NamaBerkasDatabase = "kasir.db";

        /// <summary>Nama folder penyimpan berkas database.</summary>
        public const string NamaFolderData = "data";

        private static string? _pathDatabase;

        /// <summary>
        /// Folder tempat berkas database disimpan. Akan dibuat bila belum ada.
        /// </summary>
        public static string FolderData
        {
            get
            {
                string folder = Path.Combine(AppContext.BaseDirectory, NamaFolderData);
                try
                {
                    Directory.CreateDirectory(folder);
                }
                catch (Exception)
                {
                    // Folder tidak bisa dibuat di sini. Error yang sebenarnya
                    // akan muncul lebih jelas saat koneksi dicoba, daripada
                    // sekarang pada saat hanya membaca konfigurasi.
                }
                return folder;
            }
        }

        /// <summary>
        /// Path penuh ke berkas kasir.db.
        /// Urutan prioritas: environment variable KASIR_SMK4_DB,
        /// lalu appsettings.json, lalu folder data di samping executable.
        /// </summary>
        /// <remarks>
        /// Folder induknya selalu dibuat lebih dulu, apa pun sumber path-nya.
        /// <para>
        /// Ini bukan tambahan yang mivol. Awalnya pembuatan folder hanya
        /// dilakukan di properti FolderData, yang hanya terpakai kalau
        /// appsettings.json tidak ditemukan. Padahal appsettings.json selalu
        /// ikut ke folder aplikasi dan selalu menunjuk ke "data\kasir.db",
        /// jadi FolderData tidak pernah dipanggil sama sekali.
        /// </para>
        /// <para>
        /// Akibatnya folder data tidak pernah terbentuk. Driver SQLite dengan
        /// Mode=ReadWriteCreate memang membuat BERKAS yang hilang, tetapi
        /// tidak membuat FOLDER. Koneksi lalu gagal dengan pesan "unable to
        /// open database file", dan yang terlihat oleh pengguna hanyalah
        /// jendela login yang tidak bisa dipakai. Berkas kasir.db yang tidak
        /// pernah ada membuat penyebabnya sulit dilacak dari luar.
        /// </para>
        /// </remarks>
        public static string GetPathDatabase()
        {
            if (_pathDatabase is not null)
            {
                return _pathDatabase;
            }

            string? dariEnvironment = Environment.GetEnvironmentVariable("KASIR_SMK4_DB");
            if (!string.IsNullOrWhiteSpace(dariEnvironment))
            {
                return Simpan(Normalisasi(dariEnvironment));
            }

            string pathConfig = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            if (File.Exists(pathConfig))
            {
                try
                {
                    using JsonDocument dokumen = JsonDocument.Parse(File.ReadAllText(pathConfig));
                    if (dokumen.RootElement.TryGetProperty("Database", out JsonElement db)
                        && db.TryGetProperty("Path", out JsonElement path)
                        && path.ValueKind == JsonValueKind.String)
                    {
                        string? nilai = path.GetString();
                        if (!string.IsNullOrWhiteSpace(nilai))
                        {
                            return Simpan(Normalisasi(nilai));
                        }
                    }
                }
                catch (JsonException)
                {
                    // Berkas tidak valid -> pakai nilai bawaan
                }
                catch (IOException)
                {
                    // Berkas tidak bisa dibaca -> pakai nilai bawaan
                }
            }

            return Simpan(Path.Combine(FolderData, NamaBerkasDatabase));
        }

        /// <summary>
        /// Membuat folder induk bila belum ada, lalu menyimpan path hasil
        /// perhitungan agar tidak diulang pada setiap panggilan.
        /// </summary>
        private static string Simpan(string path)
        {
            SiapkanFolderInduk(path);
            _pathDatabase = path;
            return _pathDatabase;
        }

        /// <summary>
        /// Membuat folder yang memuat berkas database.
        /// </summary>
        /// <remarks>
        /// Kegagalan sengaja tidak dilempar di sini. Driver SQLite akan
        /// melaporkannya dengan pesan yang jauh lebih jelas, dan pesan itu
        /// yang ditampilkan ke pengguna bersama lokasi berkasnya.
        /// </remarks>
        private static void SiapkanFolderInduk(string path)
        {
            try
            {
                string? folder = Path.GetDirectoryName(path);
                if (!string.IsNullOrWhiteSpace(folder))
                {
                    Directory.CreateDirectory(folder);
                }
            }
            catch (Exception)
            {
                // Sengaja diabaikan. Galat yang sebenarnya akan muncul
                // saat koneksi dicoba.
            }
        }

        /// <summary>
        /// Connection string SQLite yang dipakai seluruh aplikasi.
        /// </summary>
        /// <remarks>
        /// Pooling sengaja dimatikan. Secara bawaan Microsoft.Data.Sqlite
        /// menahan koneksi di pool sehingga berkas kasir.db tetap terkunci
        /// beberapa detik setelah aplikasi ditutup. Untuk aplikasi yang
        /// datanya harus bisa langsung disalin ke flashdisk, berkas yang
        /// masih terkunci justru menghambat: berkas tidak bisa disalin dan
        /// muncul galat "being used by another process". Sudah dibuktikan di
        /// uji: dengan Pooling=True berkas tidak bisa dihapus setelah Close,
        /// dengan Pooling=False langsung bisa.
        /// <para>
        /// Cache=Private mencegah SQLite memakai berkas -wal atau -shm di
        /// folder yang sama. Itu penting karena flashdisk sering diformat
        /// FAT32 yang tidak mendukung file journal, dan karena seluruh isi
        /// folder aplikasi disalin mentah saat pemindahan data.
        /// </para>
        /// </remarks>
        public static string GetConnectionString()
        {
            var pembangun = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder
            {
                DataSource = GetPathDatabase(),
                Pooling = false,
                Cache = Microsoft.Data.Sqlite.SqliteCacheMode.Private,
                Mode = Microsoft.Data.Sqlite.SqliteOpenMode.ReadWriteCreate
            };
            return pembangun.ToString();
        }

        /// <summary>
        /// Mengubah path relatif menjadi path penuh terhadap folder aplikasi.
        /// Path yang sudah berupa path absolut dipakai apa adanya.
        /// </summary>
        private static string Normalisasi(string nilai)
        {
            string bersih = nilai.Trim().Trim('"');
            if (Path.IsPathRooted(bersih))
            {
                return Path.GetFullPath(bersih);
            }
            return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, bersih));
        }
    }
}
