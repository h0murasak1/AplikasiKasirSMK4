using Microsoft.Data.Sqlite;

namespace AplikasiKasirSMK4
{
    /// <summary>
    /// Mengubah galat SQLite menjadi kategori yang bisa dibaca manusia,
    /// supaya berkas SQL tidak perlu menulis angka kode galat secara
    /// membumi di dalam blok catch.
    /// </summary>
    /// <remarks>
    /// Driver MySQL memakai satu angka per jenis kesalahan, misalnya 1062
    /// untuk kunci duplikat dan 1452 untuk foreign key. SQLite tidak
    /// seperti itu. Semuanya dilaporkan sebagai kode 19 (SQLITE_CONSTRAINT),
    /// lalu dibedakan lewat SqliteExtendedErrorCode atau lewat teks pesan.
    /// <para>
    /// Kode yang dipakai di sini sudah diuji, bukan diterka:
    ///   duplikat PRIMARY KEY -> 19 / 1555
    ///   duplikat UNIQUE      -> 19 / 2067
    ///   CHECK gagal         -> 19 / 275
    /// </para>
    /// </remarks>
    internal static class SqliteError
    {
        public enum Jenis
        {
            /// <summary>Galat lain yang tidak termasuk kategori apa pun.</summary>
            Lainnya = 0,

            /// <summary>Nilai dikosongkan padahal kolomnya melarang null.</summary>
            TidakBolehNull,

            /// <summary>Nilai melanggar CHECK constraint.</summary>
            Check,

            /// <summary>Foreign key tidak ditemukan, atau masih dipakai.</summary>
            ForeignKey,

            /// <summary>Kunci utama atau UNIQUE bentrok. Inilah "sudah dipakai".</summary>
            Unik
        }

        /// <summary>
        /// Menentukan kategori galat dari sebuah SqliteException.
        /// </summary>
        public static Jenis AmbilJenis(SqliteException ex)
        {
            // Hanya galat constraint (19) yang punya kategori. Galat lain,
            // misalnya database terkunci atau tabel tidak ada, dikembalikan
            // apa adanya supaya blok catch di atasnya tetap bekerja.
            if (ex.SqliteErrorCode != 19)
            {
                return Jenis.Lainnya;
            }

            switch (ex.SqliteExtendedErrorCode)
            {
                case 1555:   // SQLITE_CONSTRAINT_PRIMARYKEY
                case 2067:   // SQLITE_CONSTRAINT_UNIQUE
                    return Jenis.Unik;
                case 1299:   // SQLITE_CONSTRAINT_NOTNULL
                    return Jenis.TidakBolehNull;
                case 787:    // SQLITE_CONSTRAINT_FOREIGNKEY
                    return Jenis.ForeignKey;
                case 275:    // SQLITE_CONSTRAINT_CHECK
                    return Jenis.Check;
            }

            // Kode yang tidak dikenal atau tidak dilaporkan. SQLite
            // menulis jenis pelanggaran ke dalam teks pesan, jadi teks itu
            // tetap bisa dibaca sebagai cadangan terakhir.
            string pesan = ex.Message;
            if (pesan.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase)
                || pesan.Contains("PRIMARY KEY", StringComparison.OrdinalIgnoreCase))
            {
                return Jenis.Unik;
            }
            if (pesan.Contains("CHECK", StringComparison.OrdinalIgnoreCase))
            {
                return Jenis.Check;
            }
            if (pesan.Contains("FOREIGN KEY", StringComparison.OrdinalIgnoreCase))
            {
                return Jenis.ForeignKey;
            }
            if (pesan.Contains("NOT NULL", StringComparison.OrdinalIgnoreCase))
            {
                return Jenis.TidakBolehNull;
            }

            return Jenis.Lainnya;
        }

        /// <summary>
        /// Menyatakan apakah galat ini berarti "nilai keunikan sudah dipakai".
        /// </summary>
        public static bool AdalahUnik(SqliteException ex) => AmbilJenis(ex) == Jenis.Unik;

        /// <summary>
        /// Menyatakan apakah galat ini berarti sebuah CHECK yang ada di
        /// skema menolak nilai yang dikirim.
        /// </summary>
        /// <remarks>
        /// Galat CHECK dikembalikan sebagai kode 19 dengan kode tambahan
        /// 275. Bentuk itu sama dengan pelanggaran keunikan di sisi lain,
        /// jadi tidak bisa dibedakan dari kode galat saja; yang dipakai
        /// adalah kata "CHECK" di pesan yang dikirim SQLite.
        /// <para>
        /// Kegunaannya untuk pesan kasir: "harga tidak boleh negatif"
        /// jauh lebih berguna daripada "constraint failed: chk_transaksi_total".
        /// </para>
        /// </remarks>
        public static bool AdalahCheck(SqliteException ex)
        {
            return AmbilJenis(ex) != Jenis.Unik
                && ex.Message.Contains("CHECK", StringComparison.OrdinalIgnoreCase);
        }
    }
}
