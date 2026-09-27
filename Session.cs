namespace AplikasiKasirSMK4
{
    /// <summary>
    /// Menyimpan data user yang sedang login agar tidak perlu login berulang kali
    /// dan agar id_user pada transaksi benar-benar berasal dari user tersebut.
    /// </summary>
    internal static class Session
    {
        public static int UserId { get; private set; }
        public static string Username { get; private set; } = string.Empty;
        public static string FullName { get; private set; } = string.Empty;
        public static string NamaLengkap => FullName;
        public static string Role { get; private set; } = string.Empty;

        public static bool IsLoggedIn => UserId > 0;

        public static bool IsAdmin =>
            string.Equals(Role, "Admin", StringComparison.OrdinalIgnoreCase);

        public static bool IsKasir =>
            string.Equals(Role, "Kasir", StringComparison.OrdinalIgnoreCase);

        public static void Set(int id, string username, string fullName, string role)
        {
            UserId = id;
            Username = username;
            FullName = fullName;
            Role = role;
        }

        public static void Clear()
        {
            UserId = 0;
            Username = string.Empty;
            FullName = string.Empty;
            Role = string.Empty;
        }
    }
}
