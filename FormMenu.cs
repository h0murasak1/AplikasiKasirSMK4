namespace AplikasiKasirSMK4
{
    public partial class FormMenu : Form
    {
        private bool _sedangLogout;

        public FormMenu()
        {
            InitializeComponent();
            Load += FormMenu_Load;
            FormClosing += FormMenu_FormClosing;
        }

        private void FormMenu_Load(object? sender, EventArgs e)
        {
            // Guard: dashboard ini hanya boleh dibuka oleh role Admin.
            if (!Session.IsAdmin)
            {
                MessageBox.Show(
                    "Hanya akun Admin yang dapat mengakses menu ini.",
                    "Akses Ditolak",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                Close();
            }
        }

        private void btnInputBarang_Click(object sender, EventArgs e)
        {
            // Cegah jendela master barang terbuka berulang kali.
            if (SudahTerbuka(typeof(FormBarang)))
            {
                return;
            }

            new FormBarang().Show(this);
        }

        private void btnKasir_Click(object sender, EventArgs e)
        {
            if (SudahTerbuka(typeof(FormKasir)))
            {
                return;
            }

            // Dibuka dari dashboard, jadi bukan form root (IsRootForm = false).
            new FormKasir { IsRootForm = false }.Show(this);
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            DialogResult konfirmasi = MessageBox.Show(
                "Anda yakin ingin logout dari aplikasi?",
                "Konfirmasi Logout",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (konfirmasi != DialogResult.Yes)
            {
                return;
            }

            _sedangLogout = true;
            Session.Clear();

            // Kembali ke layar login.
            new Form1().Show();
            Close();
        }

        private void FormMenu_FormClosing(object? sender, FormClosingEventArgs e)
        {
            // Menutup dashboard dengan tombol X berarti keluar dari aplikasi.
            if (!_sedangLogout)
            {
                Session.Clear();
                Application.Exit();
            }
        }

        /// <summary>Mencegah form yang sama dibuka lebih dari sekali.</summary>
        private static bool SudahTerbuka(Type tipeForm)
        {
            foreach (Form f in Application.OpenForms)
            {
                if (f.GetType() == tipeForm && f.Visible)
                {
                    f.Activate();
                    f.BringToFront();
                    return true;
                }
            }

            return false;
        }
    }
}
