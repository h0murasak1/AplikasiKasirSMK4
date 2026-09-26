using System.Text.Json;

namespace AplikasiKasirSMK4
{
    /// <summary>
    /// Membaca konfigurasi aplikasi (connection string) dari appsettings.json.
    /// Credential database TIDAK lagi ditulis di dalam kode C#.
    /// </summary>
    internal static class AppConfig
    {
        private const string DefaultConnectionString =
            "Server=localhost;Database=db_kasir_smk4;Uid=root;Pwd=;";

        private static string? _connectionString;

        /// <summary>
        /// Urutan prioritas: environment variable KASIR_SMK4_CONNECTION,
        /// lalu appsettings.json, lalu nilai bawaan.
        /// </summary>
        public static string GetConnectionString()
        {
            if (_connectionString is not null)
            {
                return _connectionString;
            }

            string? dariEnvironment = Environment.GetEnvironmentVariable("KASIR_SMK4_CONNECTION");
            if (!string.IsNullOrWhiteSpace(dariEnvironment))
            {
                _connectionString = dariEnvironment.Trim();
                return _connectionString;
            }

            string path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            if (File.Exists(path))
            {
                try
                {
                    using JsonDocument dokumen = JsonDocument.Parse(File.ReadAllText(path));
                    if (dokumen.RootElement.TryGetProperty("ConnectionStrings", out JsonElement cs)
                        && cs.TryGetProperty("MySql", out JsonElement mySql))
                    {
                        string? nilai = mySql.GetString();
                        if (!string.IsNullOrWhiteSpace(nilai))
                        {
                            _connectionString = nilai.Trim();
                            return _connectionString;
                        }
                    }
                }
                catch (JsonException)
                {
                    // File tidak valid -> pakai nilai bawaan
                }
                catch (IOException)
                {
                    // File tidak bisa dibaca -> pakai nilai bawaan
                }
            }

            _connectionString = DefaultConnectionString;
            return _connectionString;
        }
    }
}
