using System.Security.Cryptography;
using System.Text;

namespace AplikasiKasirSMK4
{
    /// <summary>
    /// Hashing password memakai PBKDF2-SHA256 (bawaan .NET, tanpa paket tambahan).
    ///
    /// Format tersimpan : PBKDF2$iterasi$saltBase64$hashBase64
    ///
    /// Password lama yang masih tersimpan sebagai teks biasa (plain text) tetap
    /// bisa login, lalu otomatis di-upgrade menjadi hash saat login berhasil.
    /// </summary>
    internal static class PasswordHasher
    {
        private const string Prefix = "PBKDF2";
        private const int SaltSize = 16;      // 128 bit
        private const int KeySize = 32;      // 256 bit
        private const int Iterations = 100_000;

        /// <summary>Menghasilkan hash baru dari password teks biasa.</summary>
        public static string Hash(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] key = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                KeySize);

            return string.Join('$', Prefix, Iterations.ToString(),
                Convert.ToBase64String(salt), Convert.ToBase64String(key));
        }

        /// <summary>True jika password tersimpan sudah berupa hash.</summary>
        public static bool IsHashed(string? tersimpan)
        {
            return !string.IsNullOrEmpty(tersimpan)
                && tersimpan.StartsWith(Prefix + "$", StringComparison.Ordinal);
        }

        /// <summary>
        /// Memverifikasi password. Otomatis menangani password lama (plain text).
        /// </summary>
        public static bool Verify(string password, string? tersimpan)
        {
            if (string.IsNullOrEmpty(tersimpan))
            {
                return false;
            }

            // Password lama: teks biasa.
            if (!IsHashed(tersimpan))
            {
                return string.Equals(password, tersimpan, StringComparison.Ordinal);
            }

            string[] bagian = tersimpan.Split('$');
            if (bagian.Length != 4
                || !int.TryParse(bagian[1], out int iterasi)
                || iterasi <= 0)
            {
                return false;
            }

            byte[] salt;
            byte[] hashHarapan;
            try
            {
                salt = Convert.FromBase64String(bagian[2]);
                hashHarapan = Convert.FromBase64String(bagian[3]);
            }
            catch (FormatException)
            {
                return false;
            }

            if (salt.Length == 0 || hashHarapan.Length == 0)
            {
                return false;
            }

            byte[] hashAsli = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                iterasi,
                HashAlgorithmName.SHA256,
                hashHarapan.Length);

            // Perbandingan waktu konstan agar tidak bocor lewat timing attack.
            return CryptographicOperations.FixedTimeEquals(hashAsli, hashHarapan);
        }
    }
}
