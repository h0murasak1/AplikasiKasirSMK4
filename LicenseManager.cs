using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace AplikasiKasirSMK4
{
    public class LicenseData
    {
        public string HardwareId { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string Tipe { get; set; } = "PERPETUAL";
        public string? TanggalKadaluarsa { get; set; }
        public DateTime WaktuAktivasi { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Mengelola verifikasi, validasi, dan penyimpanan lisensi perangkat (Node-Locking).
    /// </summary>
    public static class LicenseManager
    {
        // Kunci rahasia milik Developer Farhan untuk menandatangani lisensi
        private const string SecretKey = "Farhan_SMK4_POS_MasterSecret_Key_2026_@k$!r";

        private static readonly string FolderProgramData =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "KasirSMK4");

        private static readonly string FolderAppData =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KasirSMK4");

        private const string NamaBerkasLisensi = "license.lic";

        /// <summary>
        /// Memeriksa apakah perangkat saat ini sudah teraktivasi secara sah.
        /// </summary>
        public static bool PeriksaStatusAktivasi(out string pesanStatus)
        {
            string currentHwId = HardwareIdHelper.AmbilHardwareId();

            // Cek berkas lisensi di ProgramData atau LocalAppData
            string path1 = Path.Combine(FolderProgramData, NamaBerkasLisensi);
            string path2 = Path.Combine(FolderAppData, NamaBerkasLisensi);

            string? pathDitemukan = File.Exists(path1) ? path1 : (File.Exists(path2) ? path2 : null);

            if (pathDitemukan == null)
            {
                pesanStatus = "Aplikasi belum teraktivasi pada perangkat ini.";
                return false;
            }

            try
            {
                string encrypted = File.ReadAllText(pathDitemukan, Encoding.UTF8);
                string json = Dekripsi(encrypted, currentHwId);
                var data = JsonSerializer.Deserialize<LicenseData>(json);

                if (data == null)
                {
                    pesanStatus = "Berkas lisensi rusak atau tidak terbaca.";
                    return false;
                }

                // 1. Verifikasi kecocokan Hardware ID
                if (!string.Equals(data.HardwareId, currentHwId, StringComparison.OrdinalIgnoreCase))
                {
                    pesanStatus = "Lisensi tidak cocok dengan perangkat ini (terdeteksi pemindahan folder ke PC lain).";
                    return false;
                }

                // 2. Verifikasi keabsahan Kunci Aktivasi
                if (!ValidasiKunci(currentHwId, data.Key, out string tipeLisensi, out DateTime? tglKadaluarsa))
                {
                    pesanStatus = "Kunci lisensi tidak valid.";
                    return false;
                }

                // 3. Cek Masa Berlaku jika bukan Perpetual
                if (tglKadaluarsa.HasValue && DateTime.Today > tglKadaluarsa.Value)
                {
                    pesanStatus = $"Masa aktif lisensi telah berakhir pada {tglKadaluarsa.Value:dd MMMM yyyy}. Silakan hubungi Developer.";
                    return false;
                }

                pesanStatus = tglKadaluarsa.HasValue
                    ? $"Lisensi Aktif hingga {tglKadaluarsa.Value:dd/MM/yyyy}"
                    : "Lisensi Permanen Aktif";
                return true;
            }
            catch
            {
                pesanStatus = "Gagal memverifikasi lisensi perangkat.";
                return false;
            }
        }

        /// <summary>
        /// Melakukan aktivasi dengan kunci yang diinputkan pengguna.
        /// </summary>
        public static bool Aktivasi(string kunciInput, out string pesanHasil)
        {
            string currentHwId = HardwareIdHelper.AmbilHardwareId();
            kunciInput = kunciInput.Trim().ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(kunciInput))
            {
                pesanHasil = "Kunci aktivasi tidak boleh kosong.";
                return false;
            }

            if (!ValidasiKunci(currentHwId, kunciInput, out string tipeLisensi, out DateTime? tglKadaluarsa))
            {
                pesanHasil = "Kunci aktivasi salah atau tidak sesuai untuk perangkat ini!\nPastikan Anda meminta kunci aktivasi resmi ke Developer.";
                return false;
            }

            if (tglKadaluarsa.HasValue && DateTime.Today > tglKadaluarsa.Value)
            {
                pesanHasil = $"Kunci aktivasi ini sudah kadaluarsa sejak {tglKadaluarsa.Value:dd MMMM yyyy}.";
                return false;
            }

            // Simpan data lisensi
            var data = new LicenseData
            {
                HardwareId = currentHwId,
                Key = kunciInput,
                Tipe = tipeLisensi,
                TanggalKadaluarsa = tglKadaluarsa?.ToString("yyyy-MM-dd"),
                WaktuAktivasi = DateTime.Now
            };

            try
            {
                string json = JsonSerializer.Serialize(data);
                string encrypted = Enkripsi(json, currentHwId);

                // Tulis ke kedua lokasi (ProgramData & LocalAppData)
                TulisBerkasAman(Path.Combine(FolderProgramData, NamaBerkasLisensi), encrypted);
                TulisBerkasAman(Path.Combine(FolderAppData, NamaBerkasLisensi), encrypted);

                pesanHasil = "Aktivasi berhasil! Aplikasi kini terdaftar secara resmi untuk perangkat ini.";
                return true;
            }
            catch (Exception ex)
            {
                pesanHasil = "Gagal menyimpan lisensi: " + ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Menghasilkan Kunci Aktivasi untuk suatu Hardware ID (Digunakan oleh Keygen Developer).
        /// </summary>
        public static string GenerateKunci(string hardwareId, DateTime? tanggalExpired = null)
        {
            hardwareId = hardwareId.Trim().ToUpperInvariant();

            if (tanggalExpired.HasValue)
            {
                string expStr = tanggalExpired.Value.ToString("yyyyMMdd");
                string payload = $"{hardwareId}|EXP:{expStr}";
                string hashHex = HitungHmac(payload, SecretKey);
                // ACT4-E-YYYYMMDD-XXXX-XXXX-XXXX (12 hex chars dari HMAC)
                return $"ACT4-E-{expStr}-{hashHex.Substring(0, 4)}-{hashHex.Substring(4, 4)}-{hashHex.Substring(8, 4)}";
            }
            else
            {
                string payload = $"{hardwareId}|PERPETUAL";
                string hashHex = HitungHmac(payload, SecretKey);
                // ACT4-P-XXXX-XXXX-XXXX-XXXX (16 hex chars dari HMAC)
                return $"ACT4-P-{hashHex.Substring(0, 4)}-{hashHex.Substring(4, 4)}-{hashHex.Substring(8, 4)}-{hashHex.Substring(12, 4)}";
            }
        }

        /// <summary>
        /// Memvalidasi format dan tanda tangan kriptografis kunci aktivasi terhadap Hardware ID perangkat.
        /// </summary>
        public static bool ValidasiKunci(string hardwareId, string kunci, out string tipeLisensi, out DateTime? tglKadaluarsa)
        {
            tipeLisensi = "INVALID";
            tglKadaluarsa = null;

            kunci = kunci.Trim().ToUpperInvariant();
            hardwareId = hardwareId.Trim().ToUpperInvariant();

            // Format 1: ACT4-P-XXXX-XXXX-XXXX-XXXX (Permanen)
            if (kunci.StartsWith("ACT4-P-"))
            {
                string expectedKey = GenerateKunci(hardwareId, null);
                if (string.Equals(kunci, expectedKey, StringComparison.OrdinalIgnoreCase))
                {
                    tipeLisensi = "PERPETUAL";
                    return true;
                }
            }
            // Format 2: ACT4-E-YYYYMMDD-XXXX-XXXX-XXXX (Berjangka)
            else if (kunci.StartsWith("ACT4-E-"))
            {
                string[] parts = kunci.Split('-');
                if (parts.Length == 6 && parts[2].Length == 8)
                {
                    string dateStr = parts[2];
                    if (DateTime.TryParseExact(dateStr, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out DateTime expDate))
                    {
                        string expectedKey = GenerateKunci(hardwareId, expDate);
                        if (string.Equals(kunci, expectedKey, StringComparison.OrdinalIgnoreCase))
                        {
                            tipeLisensi = "TEMPORARY";
                            tglKadaluarsa = expDate;
                            return true;
                        }
                    }
                }
            }
            // Format 3: ACT4-XXXX-XXXX-XXXX-XXXX (Legacy alias untuk Permanen)
            else if (kunci.StartsWith("ACT4-"))
            {
                string permKey = GenerateKunci(hardwareId, null);
                string shortExpected = permKey.Replace("ACT4-P-", "ACT4-");
                if (string.Equals(kunci, shortExpected, StringComparison.OrdinalIgnoreCase))
                {
                    tipeLisensi = "PERPETUAL";
                    return true;
                }
            }

            return false;
        }

        private static string HitungHmac(string data, string secret)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(secret);
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            using var hmac = new HMACSHA256(keyBytes);
            byte[] hash = hmac.ComputeHash(dataBytes);
            return Convert.ToHexString(hash).ToUpperInvariant();
        }

        private static void TulisBerkasAman(string targetPath, string isi)
        {
            string? dir = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            File.WriteAllText(targetPath, isi, Encoding.UTF8);
        }

        private static string Enkripsi(string plainText, string hwKey)
        {
            byte[] key = SHA256.HashData(Encoding.UTF8.GetBytes(hwKey + SecretKey));
            byte[] iv = MD5.HashData(Encoding.UTF8.GetBytes(hwKey));

            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;

            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs, Encoding.UTF8))
            {
                sw.Write(plainText);
            }

            return Convert.ToBase64String(ms.ToArray());
        }

        private static string Dekripsi(string cipherText, string hwKey)
        {
            byte[] key = SHA256.HashData(Encoding.UTF8.GetBytes(hwKey + SecretKey));
            byte[] iv = MD5.HashData(Encoding.UTF8.GetBytes(hwKey));

            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;

            byte[] buffer = Convert.FromBase64String(cipherText);
            using var ms = new MemoryStream(buffer);
            using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var sr = new StreamReader(cs, Encoding.UTF8);
            return sr.ReadToEnd();
        }
    }
}
