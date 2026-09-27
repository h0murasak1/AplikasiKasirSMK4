namespace AplikasiKasirSMK4
{
    public partial class FormMenu : Form
    {
        private bool _sedangLogout;
        private Form? _formAktif;
        private Button? _tombolAktif;

        private readonly Color WarnaTombolAktif = Color.FromArgb(41, 128, 185);
        private readonly Color WarnaTombolNonaktif = Color.FromArgb(52, 73, 94);
        private readonly Color WarnaTeksAktif = Color.White;
        private readonly Color WarnaTeksNonaktif = Color.FromArgb(200, 210, 220);

        public FormMenu()
        {
            InitializeComponent();
            UiThemeHelper.TerapkanIkon(this);
            Load += FormMenu_Load;
            FormClosing += FormMenu_FormClosing;
        }

        private void FormMenu_Load(object? sender, EventArgs e)
        {
            UiThemeHelper.TerapkanIkon(this);

            if (!Session.IsAdmin)
            {
                MessageBox.Show(
                    "Hanya akun Admin yang dapat mengakses menu ini.",
                    "Akses Ditolak",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                Close();
                return;
            }

            Image? logo = UiThemeHelper.AmbilLogoSekolah();
            if (logo != null)
            {
                picBrandLogo.Image = logo;
            }

            lblUserStatus.Text = $"👤 {Session.NamaLengkap} (Admin)";

            // Buka Kasir secara default saat menu dashboard dibuka
            BukaFormDalamPanel(new FormKasir { IsRootForm = false }, navKasir);
        }

        /// <summary>
        /// Membuka form langsung di dalam panel konten dashboard tanpa membuat jendela baru.
        /// </summary>
        private void BukaFormDalamPanel(Form childForm, Button tombolNavigasi)
        {
            if (_formAktif != null)
            {
                _formAktif.Close();
                _formAktif.Dispose();
            }

            _formAktif = childForm;
            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;

            panelKonten.Controls.Clear();
            panelKonten.Controls.Add(childForm);
            panelKonten.Tag = childForm;
            childForm.BringToFront();
            childForm.Show();

            SorotTombolNavigasi(tombolNavigasi);
        }

        private void SorotTombolNavigasi(Button tombolDipilih)
        {
            Button[] semuaTombol = new[]
            {
                navKasir, navBarang, navLaporan, navStok,
                navPiutang, navKomisi, navMaster, navBackup
            };

            foreach (Button b in semuaTombol)
            {
                b.BackColor = WarnaTombolNonaktif;
                b.ForeColor = WarnaTeksNonaktif;
            }

            tombolDipilih.BackColor = WarnaTombolAktif;
            tombolDipilih.ForeColor = WarnaTeksAktif;
            _tombolAktif = tombolDipilih;
        }

        private void navKasir_Click(object? sender, EventArgs e)
        {
            BukaFormDalamPanel(new FormKasir { IsRootForm = false }, navKasir);
        }

        private void navBarang_Click(object? sender, EventArgs e)
        {
            BukaFormDalamPanel(new FormBarang(), navBarang);
        }

        private void navLaporan_Click(object? sender, EventArgs e)
        {
            BukaFormDalamPanel(new FormLaporan(), navLaporan);
        }

        private void navStok_Click(object? sender, EventArgs e)
        {
            BukaFormDalamPanel(new FormStok(), navStok);
        }

        private void navPiutang_Click(object? sender, EventArgs e)
        {
            BukaFormDalamPanel(new FormPiutang(), navPiutang);
        }

        private void navKomisi_Click(object? sender, EventArgs e)
        {
            BukaFormDalamPanel(new FormKomisi(), navKomisi);
        }

        private void navMaster_Click(object? sender, EventArgs e)
        {
            BukaFormDalamPanel(new FormMasterMenu(), navMaster);
        }

        private void navBackup_Click(object? sender, EventArgs e)
        {
            BukaFormDalamPanel(new FormBackup(), navBackup);
        }

        private void btnLogout_Click(object? sender, EventArgs e)
        {
            DialogResult konfirmasi = MessageBox.Show(
                "Anda yakin ingin logout dari aplikasi?",
                "Konfirmasi Logout",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (konfirmasi != DialogResult.Yes) return;

            _sedangLogout = true;

            if (_formAktif != null)
            {
                _formAktif.Close();
                _formAktif.Dispose();
            }

            Session.Clear();
            new Form1().Show();
            Close();
        }

        private void FormMenu_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (!_sedangLogout)
            {
                Session.Clear();
                Application.Exit();
            }
        }
    }
}
