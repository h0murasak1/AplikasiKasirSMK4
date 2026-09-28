namespace AplikasiKasirSMK4
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // Tangkap error yang tidak tertangani agar aplikasi tidak crash
            // diam-diam, melainkan menampilkan pesan yang jelas.
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (_, e) =>
                TampilkanErrorFatal(e.Exception);

            AppDomain.CurrentDomain.UnhandledException += (_, e) =>
                TampilkanErrorFatal(e.ExceptionObject as Exception);

            ApplicationConfiguration.Initialize();

            // 1. Pengecekan Lisensi Hardware-ID
            if (!LicenseManager.PeriksaStatusAktivasi(out string pesanLisensi))
            {
                using var formAktivasi = new FormAktivasi(pesanLisensi);
                if (formAktivasi.ShowDialog() != DialogResult.OK)
                {
                    // Pengguna membatalkan aktivasi atau menutup form -> Keluar dari aplikasi
                    return;
                }
            }

            // 2. Buka aplikasi jika lisensi valid
            Application.Run(new Form1());
        }

        private static void TampilkanErrorFatal(Exception? ex)
        {
            string pesan = ex?.Message ?? "Terjadi kesalahan yang tidak diketahui.";
            MessageBox.Show(
                "Aplikasi mengalami kesalahan yang tidak terduga.\n\n" + pesan,
                "ErrorFatal",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
