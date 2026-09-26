using MySql.Data.MySqlClient;

namespace AplikasiKasirSMK4
{
    /// <summary>
    /// Class untuk mengelola koneksi ke database MySQL.
    /// Connection string dibaca dari appsettings.json (lihat AppConfig.cs).
    /// </summary>
    public class Koneksi
    {
        private readonly string _connectionString;

        public Koneksi()
        {
            _connectionString = AppConfig.GetConnectionString();
        }

        public string ConnectionString => _connectionString;

        /// <summary>
        /// Mengembalikan objek koneksi baru. Selalu gunakan pola "using".
        /// </summary>
        public MySqlConnection GetConn()
        {
            return new MySqlConnection(_connectionString);
        }

        /// <summary>
        /// Menguji koneksi tanpa menampilkan MessageBox, agar pemanggil
        /// bisa menentukan sendiri cara menampilkan hasilnya.
        /// </summary>
        public bool TestConnection(out string pesanError)
        {
            try
            {
                using MySqlConnection conn = new(_connectionString);
                conn.Open();
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
