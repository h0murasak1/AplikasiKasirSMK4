namespace AplikasiKasirSMK4
{
    /// <summary>
    /// Helper terpusat untuk tema desain modern, profesional, minimalis, dan user-friendly.
    /// </summary>
    public static class UiThemeHelper
    {
        public static readonly Color NavyDark = Color.FromArgb(15, 23, 42);       // Slate 900
        public static readonly Color NavyHeader = Color.FromArgb(30, 41, 59);     // Slate 800
        public static readonly Color SlateMuted = Color.FromArgb(100, 116, 139);  // Slate 500
        public static readonly Color CardBg = Color.White;
        public static readonly Color CanvasBg = Color.FromArgb(248, 250, 252);   // Slate 50
        public static readonly Color BorderColor = Color.FromArgb(226, 232, 240); // Slate 200

        public static readonly Color Primary = Color.FromArgb(37, 99, 235);      // Royal Blue
        public static readonly Color Success = Color.FromArgb(16, 185, 129);     // Emerald
        public static readonly Color Warning = Color.FromArgb(217, 119, 6);      // Amber
        public static readonly Color Danger = Color.FromArgb(239, 68, 68);       // Rose Red
        public static readonly Color Secondary = Color.FromArgb(71, 85, 105);    // Slate 600

        private static Icon? _ikonAplikasi;

        /// <summary>
        /// Menerapkan ikon resmi SMK Negeri 4 ke Form agar muncul di Taskbar dan Titlebar.
        /// </summary>
        public static void TerapkanIkon(Form form)
        {
            try
            {
                if (_ikonAplikasi == null)
                {
                    string[] candidates =
                    [
                        Path.Combine(AppContext.BaseDirectory, "Resources", "app.ico"),
                        Path.Combine(AppContext.BaseDirectory, "..", "..", "Resources", "app.ico"),
                        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Resources", "app.ico"),
                    ];

                    foreach (string path in candidates)
                    {
                        string full = Path.GetFullPath(path);
                        if (File.Exists(full))
                        {
                            _ikonAplikasi = new Icon(full);
                            break;
                        }
                    }

                    if (_ikonAplikasi == null)
                    {
                        _ikonAplikasi = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
                    }
                }

                if (_ikonAplikasi != null)
                {
                    form.Icon = _ikonAplikasi;
                    form.ShowIcon = true;
                }
            }
            catch
            {
                // Fallback aman
            }
        }

        /// <summary>
        /// Mengambil gambar logo segel SMK Negeri 4.
        /// </summary>
        public static Image? AmbilLogoSekolah()
        {
            return AmbilGambar("logo_smk.png", "logo_smk.jpg", "app_logo.png", "app_logo.jpg");
        }

        /// <summary>
        /// Mengambil gambar banner horizontal SMK Negeri 4.
        /// </summary>
        public static Image? AmbilBannerSekolah()
        {
            return AmbilGambar("logo_banner.png", "logo_smk.png", "logo_smk.jpg");
        }

        private static Image? AmbilGambar(params string[] namaFile)
        {
            try
            {
                foreach (string nama in namaFile)
                {
                    string[] candidates =
                    [
                        Path.Combine(AppContext.BaseDirectory, "Resources", nama),
                        Path.Combine(AppContext.BaseDirectory, "..", "..", "Resources", nama),
                        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Resources", nama),
                    ];

                    foreach (string path in candidates)
                    {
                        string full = Path.GetFullPath(path);
                        if (File.Exists(full))
                        {
                            return Image.FromFile(full);
                        }
                    }
                }
            }
            catch
            {
                // Fallback aman
            }
            return null;
        }


        /// <summary>
        /// Menerapkan tema tabel modern ke DataGridView.
        /// </summary>
        public static void FormatTabel(DataGridView dgv)
        {
            dgv.BackgroundColor = Color.White;
            dgv.BorderStyle = BorderStyle.None;
            dgv.GridColor = BorderColor;
            dgv.RowHeadersVisible = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;
            dgv.AllowUserToResizeRows = false;
            dgv.RowTemplate.Height = 34;

            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgv.ColumnHeadersHeight = 38;
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            dgv.ColumnHeadersDefaultCellStyle.BackColor = NavyDark;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 4, 6, 4);

            dgv.DefaultCellStyle.BackColor = Color.White;
            dgv.DefaultCellStyle.ForeColor = Color.FromArgb(30, 41, 59);
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9F);
            dgv.DefaultCellStyle.Padding = new Padding(4, 2, 4, 2);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(224, 242, 254);
            dgv.DefaultCellStyle.SelectionForeColor = Color.FromArgb(15, 23, 42);

            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            dgv.AlternatingRowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(224, 242, 254);
            dgv.AlternatingRowsDefaultCellStyle.SelectionForeColor = Color.FromArgb(15, 23, 42);
        }

        /// <summary>
        /// Menerapkan tema tombol flat modern.
        /// </summary>
        public static void FormatTombol(Button btn, Color bg, Color fg, int height = 36)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.BackColor = bg;
            btn.ForeColor = fg;
            btn.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btn.Cursor = Cursors.Hand;
            btn.FlatAppearance.BorderSize = 0;
            btn.Height = height;
        }
    }
}
