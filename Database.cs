using System.Text;
using Microsoft.Data.Sqlite;

namespace AplikasiKasirSMK4
{
    /// <summary>
    /// Membentuk skema database saat pertama kali dijalankan, dan mengisi
    /// akun admin bawaan bila belum ada user sama sekali.
    /// </summary>
    /// <remarks>
    /// Aplikasi tidak lagi memerlukan mysql.exe, XAMPP, atau layanan
    /// database apa pun. Skema dibaca dari berkas skema.sqlite.sql yang
    /// ikut disertakan, lalu dijalankan sebagai satu rangkaian perintah.
    /// <para>
    /// Berkas SQL tidak dieksekusi sekaligus memakai ExecuteNonQuery
    /// dengan seluruh isinya, karena SQLite hanya menjalankan perintah
    /// pertama per panggilan. Isinya dipecah sendiri oleh
    /// PisahStatementSql, yang sudah diuji terhadap berkas
    /// skema.sqlite.sql dan terhadap delapan kasus kecil berisi string,
    /// trigger, dan komentar.
    /// </para>
    /// </remarks>
    internal static class Database
    {
        /// <summary>Nama berkas skema SQLite.</summary>
        public const string NamaBerkasSkema = "skema.sqlite.sql";

        /// <summary>Nama tabel yang dipakai sebagai penanda "sudah terpasang".</summary>
        public const string TabelPenanda = "tb_user";

        /// <summary>Nama tabel pencatatan versi skema.</summary>
        public const string TabelVersi = "tb_skema_versi";

        /// <summary>
        /// Versi skema yang menghasilkan berkas skema.sqlite.sql apa adanya.
        /// </summary>
        public const int VersiDasar = 1;

        /// <summary>Awalan nama berkas migrasi yang dibaca aplikasi.</summary>
        public const string AwalanMigrasi = "migrasi";

        private static bool _sudahDicek;

        /// <summary>
        /// Menjalankan pemeriksaan pemasangan hanya sekali per sesi.
        /// </summary>
        public static void PastikanTerpasang(Koneksi koneksi)
        {
            if (_sudahDicek)
            {
                return;
            }

            using SqliteConnection conn = koneksi.GetConn();

            if (!SudahTerpasang(conn))
            {
                PasangSkema(conn);
            }

            PastikanAdaAdmin(conn);
            JalankanMigrasi(conn);

            _sudahDicek = true;
        }

        /// <summary>
        /// Menyatakan apakah skema sudah terbentuk, dengan melihat apakah
        /// tabel penanda ada di sqlite_master.
        /// </summary>
        public static bool SudahTerpasang(SqliteConnection conn)
        {
            using SqliteCommand cmd = conn.CreateCommand();
            cmd.CommandText =
                "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = @t";
            cmd.Parameters.AddWithValue("@t", TabelPenanda);
            return Convert.ToInt64(cmd.ExecuteScalar()) > 0;
        }

        /// <summary>
        /// Menjalankan seluruh perintah pada berkas skema.
        /// </summary>
        private static void PasangSkema(SqliteConnection conn)
        {
            string pathSkema = CariBerkasSkema();
            string isiSkema = File.ReadAllText(pathSkema, Encoding.UTF8);

            using SqliteTransaction transaksi = conn.BeginTransaction(deferred: false);
            int dieksekusi = 0;

            foreach (string perintah in PisahStatementSql(isiSkema))
            {
                using SqliteCommand cmd = conn.CreateCommand();
                cmd.Transaction = transaksi;
                cmd.CommandText = perintah;
                cmd.ExecuteNonQuery();
                dieksekusi++;
            }

            transaksi.Commit();

            if (dieksekusi == 0)
            {
                throw new InvalidOperationException(
                    "Berkas skema " + NamaBerkasSkema + " tidak berisi satu pun perintah SQL.");
            }
        }

        /// <summary>
        /// Membuat akun admin bila tabel user masih kosong.
        /// </summary>
        /// <remarks>
        /// Password di-hash dengan salt acak, jadi akunnya tidak pernah
        /// sama di dua instalasi berbeda. Itu lebih aman daripada menulis
        /// hash yang sama ke semua toko dari satu berkas.
        /// </remarks>
        private static void PastikanAdaAdmin(SqliteConnection conn)
        {
            using (SqliteCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM tb_user";
                if (Convert.ToInt64(cmd.ExecuteScalar()) > 0)
                {
                    return;
                }
            }

            using SqliteCommand insert = conn.CreateCommand();
            insert.CommandText =
                "INSERT INTO tb_user (nama_lengkap, username, password, role, is_active) "
              + "VALUES (@nama, @user, @sandi, 'Admin', 1)";
            insert.Parameters.AddWithValue("@nama", "Administrator");
            insert.Parameters.AddWithValue("@user", "admin");
            insert.Parameters.AddWithValue("@sandi", PasswordHasher.Hash("admin123"));
            insert.ExecuteNonQuery();
        }

        // =========================================================
        //  MESIN VERSI SKEMA
        //
        //  Sebelum bagian ini, aplikasi hanya bisa memasang skema
        //  sekali. Begitu skema perlu diubah, database yang sudah ada
        //  tidak akan pernah mendapat perubahan itu, dan pemakainya
        //  harus mengulang dari nol.
        //
        //  Yang dipakai: tabel tb_skema_versi yang mencatat nomor
        //  versi terakhir, ditambah berkas-berkas di folder "migrasi"
        //  yang namanya diawali angka. Hanya skrip yang nomornya lebih
        //  besar dari versi tersimpan yang dijalankan, satu per satu,
        //  masing-masing di dalam transaksinya sendiri.
        //
        //  Kalau satu skrip gagal, transaksinya dibatalkan dan versi
        //  TIDAK dinaikkan, jadi tidak pernah ada jalur yang setengah
        //  jalan. Skrip yang gagal itu akan dicoba lagi saat aplikasi
        //  dibuka berikutnya, dan karena transaksinya sudah dibatalkan,
        //  tidak ada sisa yang perlu dibersihkan.
        // =========================================================

        /// <summary>
        /// Menjalankan seluruh migrasi yang versinya belum pernah dipasang.
        /// </summary>
        /// <returns>Nomor versi skema setelah semua migrasi dijalankan.</returns>
        public static int JalankanMigrasi(SqliteConnection conn)
        {
            PastikanAdaTabelVersi(conn);
            int sekarang = BacaVersiTerkini(conn);

            foreach (BerkasMigrasi migrasi in AmbilDaftarMigrasi())
            {
                if (migrasi.Nomor <= sekarang)
                {
                    continue;
                }

                JalankanSatuMigrasi(conn, migrasi);
                sekarang = migrasi.Nomor;
            }

            return sekarang;
        }

        /// <summary>
        /// Membuat tabel versi bila belum ada, lalu mengisi versi dasar.
        /// </summary>
        /// <remarks>
        /// Database yang dibuat sebelum tabel versi diperkenalkan tidak
        /// punya tabel ini. Isinya sudah persis skema versi 1, jadi
        /// yang perlu dilakukan hanya mencatat ulang nomor 1. Tanpa
        /// langkah ini, database lama akan menganggap versinya 0 dan
        /// menjalankan ulang seluruh migrasi dari awal.
        /// </remarks>
        public static void PastikanAdaTabelVersi(SqliteConnection conn)
        {
            using (SqliteCommand cek = conn.CreateCommand())
            {
                cek.CommandText =
                    "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = @t";
                cek.Parameters.AddWithValue("@t", TabelVersi);

                if (Convert.ToInt64(cek.ExecuteScalar()) > 0)
                {
                    return;
                }
            }

            using SqliteCommand buat = conn.CreateCommand();
            buat.CommandText =
                "CREATE TABLE IF NOT EXISTS " + TabelVersi + " ("
              + "versi INTEGER NOT NULL PRIMARY KEY,"
              + "nama TEXT NOT NULL,"
              + "dipasang_pada TEXT NOT NULL"
              + " DEFAULT (strftime('%Y-%m-%d %H:%M:%S', 'now', 'localtime')))";

            using (SqliteTransaction tx = conn.BeginTransaction(deferred: false))
            {
                buat.Transaction = tx;
                buat.ExecuteNonQuery();

                using SqliteCommand isi = conn.CreateCommand();
                isi.Transaction = tx;
                isi.CommandText =
                    "INSERT OR IGNORE INTO " + TabelVersi + " (versi, nama) VALUES (@v, @n)";
                isi.Parameters.AddWithValue("@v", VersiDasar);
                isi.Parameters.AddWithValue("@n", "Tahap 1 - skema dasar");
                isi.ExecuteNonQuery();

                tx.Commit();
            }
        }

        /// <summary>
        /// Membaca nomor versi tertinggi yang tercatat.
        /// </summary>
        /// <remarks>
        /// Mengembalikan 0 kalau tabel versinya belum ada, bukan
        /// melempar galat. Database yang dibuat sebelum tabel versi
        /// diperkenalkan sama sekali tidak punya tabel itu, dan
        /// pemanggil sering mau bertanya dulu sebelum membuatnya.
        /// </remarks>
        public static int BacaVersiTerkini(SqliteConnection conn)
        {
            using (SqliteCommand cek = conn.CreateCommand())
            {
                cek.CommandText =
                    "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = @t";
                cek.Parameters.AddWithValue("@t", TabelVersi);

                if (Convert.ToInt64(cek.ExecuteScalar()) == 0)
                {
                    return 0;
                }
            }

            using SqliteCommand cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COALESCE(MAX(versi), 0) FROM " + TabelVersi;

            object? hasil = cmd.ExecuteScalar();
            if (hasil is null || hasil is DBNull)
            {
                return 0;
            }

            return Convert.ToInt32(hasil);
        }

        /// <summary>
        /// Menjalankan satu berkas migrasi lalu mencatat versinya.
        /// </summary>
        private static void JalankanSatuMigrasi(SqliteConnection conn, BerkasMigrasi migrasi)
        {
            string isi = File.ReadAllText(migrasi.Path, Encoding.UTF8);

            using SqliteTransaction transaksi = conn.BeginTransaction(deferred: false);
            int dieksekusi = 0;

            foreach (string perintah in PisahStatementSql(isi))
            {
                using SqliteCommand cmd = conn.CreateCommand();
                cmd.Transaction = transaksi;
                cmd.CommandText = perintah;
                cmd.ExecuteNonQuery();
                dieksekusi++;
            }

            using (SqliteCommand catat = conn.CreateCommand())
            {
                catat.Transaction = transaksi;
                catat.CommandText =
                    "INSERT INTO " + TabelVersi + " (versi, nama) VALUES (@v, @n)";
                catat.Parameters.AddWithValue("@v", migrasi.Nomor);
                catat.Parameters.AddWithValue("@n", migrasi.Nama);
                catat.ExecuteNonQuery();
            }

            transaksi.Commit();

            if (dieksekusi == 0)
            {
                throw new InvalidOperationException(
                    "Berkas migrasi " + migrasi.Nama + " tidak berisi satu pun perintah SQL.");
            }
        }

        /// <summary>
        /// Mengumpulkan berkas migrasi yang tersedia, diurutkan menurut nomor.
        /// </summary>
        /// <remarks>
        /// Nama berkas harus diawali angka, misalnya 002_tahap2.sql. Angka
        /// itulah yang jadi nomor versi. Berkas yang namanya tidak diawali
        /// angka diabaikan diam-diam, karena bisa saja catatan atau
        /// berkas temporer yang tersesat di folder itu.
        /// <para>
        /// Folder migrasi dicari di beberapa lokasi, sama seperti berkas
        /// skema: folder aplikasi dulu, lalu folder sumber saat dijalankan
        /// dari Visual Studio.
        /// </para>
        /// </remarks>
        public static List<BerkasMigrasi> AmbilDaftarMigrasi()
        {
            var hasil = new List<BerkasMigrasi>();
            string? folder = CariFolderMigrasi();

            if (folder is null)
            {
                return hasil;
            }

            foreach (string path in Directory.GetFiles(folder, "*.sql"))
            {
                string nama = Path.GetFileName(path);
                if (!AmbilNomorVersi(nama, out int nomor))
                {
                    continue;
                }

                hasil.Add(new BerkasMigrasi
                {
                    Nomor = nomor,
                    Nama = nama,
                    Path = path
                });
            }

            hasil.Sort((a, b) => a.Nomor.CompareTo(b.Nomor));
            return hasil;
        }

        /// <summary>
        /// Mengambil nomor versi dari nama berkas, misalnya "002_tahap2.sql"
        /// menghasilkan 2.
        /// </summary>
        internal static bool AmbilNomorVersi(string namaBerkas, out int nomor)
        {
            nomor = 0;
            string nama = Path.GetFileNameWithoutExtension(namaBerkas);

            int i = 0;
            while (i < nama.Length && char.IsDigit(nama[i]))
            {
                i++;
            }

            // Minimal satu digit dan harus dipisah bagian lain,
            // supaya berkas bernama "2024_cadangan.sql" tidak ikut
            // terbaca sebagai versi 2024.
            if (i == 0 || i == nama.Length)
            {
                return false;
            }

            return int.TryParse(nama[..i], out nomor);
        }

        /// <summary>
        /// Mencari folder yang berisi berkas migrasi.
        /// </summary>
        internal static string? CariFolderMigrasi()
        {
            string[] kandidat =
            {
                Path.Combine(AppContext.BaseDirectory, AwalanMigrasi),
                Path.Combine(AppContext.BaseDirectory, "..", "..", AwalanMigrasi),
                Path.Combine(AppConfig.GetPathDatabase() is string db
                    ? Path.GetDirectoryName(db) ?? string.Empty
                    : string.Empty, AwalanMigrasi)
            };

            foreach (string k in kandidat)
            {
                string penuh = Path.GetFullPath(k);
                if (Directory.Exists(penuh))
                {
                    return penuh;
                }
            }

            return null;
        }

        /// <summary>
        /// Mencari berkas skema di beberapa lokasi yang mungkin.
        /// </summary>
        /// <remarks>
        /// Lokasi pertama adalah folder executable karena berkas itu ikut
        /// disalin ke folder build. Lokasi kedua dan ketiga dipakai
        /// untuk menjalankan langsung dari Visual Studio, di mana berkas
        /// sumber ada satu folder di atas folder output.
        /// </remarks>
        private static string CariBerkasSkema()
        {
            string[] kandidat =
            {
                Path.Combine(AppContext.BaseDirectory, NamaBerkasSkema),
                Path.Combine(AppContext.BaseDirectory, "sql", NamaBerkasSkema),
                Path.Combine(AppContext.BaseDirectory, "..", "..", NamaBerkasSkema)
            };

            foreach (string k in kandidat)
            {
                string penuh = Path.GetFullPath(k);
                if (File.Exists(penuh))
                {
                    return penuh;
                }
            }

            throw new FileNotFoundException(
                "Berkas skema " + NamaBerkasSkema + " tidak ditemukan. "
              + "Letakkan di folder aplikasi. Dicoba di: "
              + string.Join(", ", kandidat.Select(Path.GetFullPath)));
        }

        /// <summary>
        /// Memecah isi berkas SQL menjadi perintah-perintah yang bisa
        /// dijalankan satu per satu.
        /// </summary>
        /// <remarks>
        /// Ini bukan sekadar memecah di titik koma. Dua hal harus ditangani:
        /// <para>
        /// Komentar satu baris memakai tanda dua hubung. Tanda yang sama
        /// muncul di dalam string seperti format tanggal, jadi sebelum
        /// komentar dihapus, posisi di dalam string harus dikenali lebih dulu.
        /// </para>
        /// <para>
        /// Badan trigger boleh memuat titik koma, misalnya di dalam
        /// INSERT atau UPDATE. Kalau titik koma di situ ikut memecah
        /// perintah, trigger akan terpotong dan seluruh perintah setelahnya
        /// ikut gagal. Karena itu jumlah BEGIN dan END dihitung, dan titik
        /// koma baru dianggap akhir perintah saat tidak sedang berada di
        /// dalam badan trigger.
        /// </para>
        /// </remarks>
        public static IEnumerable<string> PisahStatementSql(string sql)
        {
            var hasil = new List<string>();
            var sb = new StringBuilder();

            int kedalaman = 0;
            bool dalamTrigger = false;
            bool dalamString = false;
            char pembatas = '\0';

            for (int i = 0; i < sql.Length; i++)
            {
                char ch = sql[i];

                if (dalamString)
                {
                    sb.Append(ch);
                    if (ch == pembatas)
                    {
                        // Dua pembatas berturut-turut berarti pembatas yang
                        // benar-benar ada di dalam string, bukan penutup.
                        if (i + 1 < sql.Length && sql[i + 1] == pembatas)
                        {
                            sb.Append(sql[++i]);
                            continue;
                        }
                        dalamString = false;
                    }
                    continue;
                }

                if (ch == '\'' || ch == '"')
                {
                    dalamString = true;
                    pembatas = ch;
                    sb.Append(ch);
                    continue;
                }

                if (ch == '-' && i + 1 < sql.Length && sql[i + 1] == '-')
                {
                    while (i < sql.Length && sql[i] != '\n')
                    {
                        i++;
                    }
                    sb.Append('\n');
                    continue;
                }

                if (char.IsLetter(ch) || ch == '_')
                {
                    int mulai = i;
                    while (i < sql.Length && (char.IsLetterOrDigit(sql[i]) || sql[i] == '_'))
                    {
                        i++;
                    }
                    string kata = sql[mulai..i];

                    if (string.Equals(kata, "TRIGGER", StringComparison.OrdinalIgnoreCase))
                    {
                        dalamTrigger = true;
                    }
                    else if (dalamTrigger)
                    {
                        if (string.Equals(kata, "BEGIN", StringComparison.OrdinalIgnoreCase))
                        {
                            kedalaman++;
                        }
                        else if (string.Equals(kata, "END", StringComparison.OrdinalIgnoreCase))
                        {
                            kedalaman--;
                        }
                    }

                    sb.Append(kata);
                    i--;
                    continue;
                }

                bool diDalamBadanTrigger = dalamTrigger && kedalaman > 0;
                if (ch == ';' && !diDalamBadanTrigger)
                {
                    string perintah = sb.ToString().Trim();
                    if (perintah.Length > 0)
                    {
                        hasil.Add(perintah);
                    }
                    sb.Clear();
                    continue;
                }

                sb.Append(ch);
            }

            string ekor = sb.ToString().Trim();
            if (ekor.Length > 0)
            {
                hasil.Add(ekor);
            }

            return hasil;
        }
    }
}
