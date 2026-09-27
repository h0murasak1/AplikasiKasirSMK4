using Microsoft.Data.Sqlite;

namespace AplikasiKasirSMK4
{
    /// <summary>
    /// Class untuk mengelola koneksi ke database SQLite.
    /// Lokasi berkas database dibaca dari AppConfig.cs.
    /// </summary>
    /// <remarks>
    /// Semua query aplikasi memakai pola parameter, tidak ada string SQL yang
    /// dirakit dari input pengguna. Itu tetap berlaku setelah pindah ke
    /// SQLite. Perbedaan penting dari MySQL: parameter SQLite ditulis dengan
    /// awalan yang sama, yaitu "@nama", jadi tidak ada query yang perlu
    /// ditulis ulang hanya karena pindah driver.
    /// </remarks>
    public class Koneksi
    {
        private readonly string _connectionString;

        public Koneksi()
        {
            _connectionString = AppConfig.GetConnectionString();
        }

        /// <summary>Path penuh ke berkas database yang sedang dipakai.</summary>
        public string PathDatabase => AppConfig.GetPathDatabase();

        /// <summary>Connection string SQLite yang sedang dipakai.</summary>
        public string ConnectionString => _connectionString;

        /// <summary>
        /// Mengembalikan objek koneksi baru. Selalu gunakan pola "using".
        /// </summary>
        public SqliteConnection GetConn()
        {
            var conn = new SqliteConnection(_connectionString);
            conn.Open();
            TerapkanPragma(conn);
            return conn;
        }

        /// <summary>
        /// Menjalankan PRAGMA yang wajib berlaku di setiap koneksi.
        /// </summary>
        /// <remarks>
        /// PRAGMA bersifat per-koneksi, bukan per-berkas, jadi tidak bisa
        /// cukup dipasang sekali di awal aplikasi. Koneksi baru akan
        /// kembali ke setelan bawaan, dan setelan bawaan SQLite untuk
        /// foreign_keys adalah MATI. Kalau sakelarnya lolos, penghapusan
        /// barang akan meninggalkan baris mutasi stok dan nota yatim
        /// yang tidak dianggap sebagai kesalahan oleh database.
        /// <para>
        /// recursive_triggers sengaja dimatikan. Trigger cap waktu pada
        /// tb_barang menjalankan UPDATE pada tabelnya sendiri, jadi kalau
        /// recursive_triggers nyala trigger itu memanggil dirinya sendiri
        /// tanpa henti. Lihat komentar panjang di skema.sqlite.sql.
        /// </para>
        /// </remarks>
        private static void TerapkanPragma(SqliteConnection conn)
        {
            using SqliteCommand cmd = conn.CreateCommand();
            cmd.CommandText =
                "PRAGMA foreign_keys = ON;"
              + "PRAGMA recursive_triggers = OFF;"
              + "PRAGMA busy_timeout = 5000;";
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Menguji koneksi tanpa menampilkan MessageBox, agar pemanggil
        /// bisa menentukan sendiri cara menampilkan hasilnya.
        /// </summary>
        public bool TestConnection(out string pesanError)
        {
            try
            {
                using SqliteConnection conn = GetConn();
                pesanError = string.Empty;
                return true;
            }
            catch (Exception ex)
            {
                pesanError = ex.Message;
                return false;
            }
        }
    }
}
