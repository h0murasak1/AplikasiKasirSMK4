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
