namespace AplikasiKasirSMK4
{
    public class FormAktivasi : Form
    {
        private readonly TextBox txtHardwareId = new();
        private readonly TextBox txtKunciAktivasi = new();
        private readonly Button btnSalin = new();
        private readonly Button btnAktivasi = new();
        private readonly Button btnKeluar = new();
        private readonly Label lblStatus = new();

        public FormAktivasi(string? pesanAwal = null)
        {
            InitializeUi(pesanAwal);
            UiThemeHelper.TerapkanIkon(this);
        }

        private void InitializeUi(string? pesanAwal)
        {
            Text = "Aktivasi Lisensi - Sistem POS SMK Negeri 4";
            Width = 600;
            Height = 520;
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.FromArgb(245, 247, 250);
            Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);

            // 1. Header Panel
            var panelHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.FromArgb(41, 128, 185)
            };

            var lblHeaderTitle = new Label
            {
                Text = "🔒 AKTIVASI LISENSI PERANGKAT",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 15),
                AutoSize = true
            };

            var lblHeaderSub = new Label
            {
                Text = "Sistem POS - SMK Negeri 4 Kabupaten Tangerang",
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(235, 245, 255),
                Location = new Point(22, 45),
                AutoSize = true
            };

            panelHeader.Controls.Add(lblHeaderTitle);
            panelHeader.Controls.Add(lblHeaderSub);
            Controls.Add(panelHeader);

            int x = 30;
            int lebar = 524;
            int y = 95;

            // 2. Info / Alert
            var lblInfo = new Label
            {
                Location = new Point(x, y),
                Size = new Size(lebar, 45),
                Text = !string.IsNullOrWhiteSpace(pesanAwal)
                    ? pesanAwal
                    : "Aplikasi ini dilindungi lisensi perangkat. Setiap PC memerlukan izin dan Kunci Aktivasi dari Developer sebelum dapat digunakan.",
                ForeColor = Color.FromArgb(80, 90, 100),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Italic)
            };
            Controls.Add(lblInfo);
            y += 50;

            // 3. Section: Kode Mesin
            var grpHardware = new GroupBox
            {
                Text = " Identitas Perangkat Komputer (Hardware ID) ",
                Location = new Point(x, y),
                Size = new Size(lebar, 105),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80)
            };

            var lblHwHint = new Label
            {
                Text = "Kirimkan kode mesin di bawah ini kepada Developer Farhan:",
                Location = new Point(16, 25),
                Size = new Size(490, 18),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(100, 110, 120)
            };
            grpHardware.Controls.Add(lblHwHint);

            string hwId = HardwareIdHelper.AmbilHardwareId();
            txtHardwareId.Text = hwId;
            txtHardwareId.Location = new Point(16, 48);
            txtHardwareId.Size = new Size(350, 32);
            txtHardwareId.Font = new Font("Consolas", 13F, FontStyle.Bold);
            txtHardwareId.ReadOnly = true;
            txtHardwareId.BackColor = Color.FromArgb(235, 245, 255);
            txtHardwareId.ForeColor = Color.FromArgb(41, 128, 185);
            txtHardwareId.TextAlign = HorizontalAlignment.Center;
            grpHardware.Controls.Add(txtHardwareId);

            btnSalin.Text = "📋 Salin Kode";
            btnSalin.Location = new Point(376, 47);
            btnSalin.Size = new Size(130, 34);
            btnSalin.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnSalin.BackColor = Color.FromArgb(52, 152, 219);
            btnSalin.ForeColor = Color.White;
            btnSalin.FlatStyle = FlatStyle.Flat;
            btnSalin.FlatAppearance.BorderSize = 0;
            btnSalin.Cursor = Cursors.Hand;
            btnSalin.Click += (_, _) =>
            {
                Clipboard.SetText(hwId);
                MessageBox.Show(
                    "Kode Mesin berhasil disalin ke clipboard:\n" + hwId + "\n\nSilakan kirimkan kode ini kepada Developer Farhan via WhatsApp.",
                    "Kode Disalin",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            };
            grpHardware.Controls.Add(btnSalin);

            Controls.Add(grpHardware);
            y += 120;

            // 4. Section: Input Kunci Aktivasi
            var grpKey = new GroupBox
            {
                Text = " Masukkan Kunci Aktivasi Resmi ",
                Location = new Point(x, y),
                Size = new Size(lebar, 100),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80)
            };

            var lblKeyHint = new Label
            {
                Text = "Tempelkan kunci aktivasi yang Anda dapatkan dari Developer:",
                Location = new Point(16, 25),
                Size = new Size(490, 18),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(100, 110, 120)
            };
            grpKey.Controls.Add(lblKeyHint);

            txtKunciAktivasi.Location = new Point(16, 48);
            txtKunciAktivasi.Size = new Size(490, 32);
            txtKunciAktivasi.Font = new Font("Consolas", 11.5F, FontStyle.Bold);
            txtKunciAktivasi.CharacterCasing = CharacterCasing.Upper;
            txtKunciAktivasi.PlaceholderText = "Contoh: ACT4-P-XXXX-XXXX-XXXX-XXXX";
            grpKey.Controls.Add(txtKunciAktivasi);

            Controls.Add(grpKey);
            y += 115;

            // Status label
            lblStatus.Location = new Point(x, y);
            lblStatus.Size = new Size(lebar, 20);
            lblStatus.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblStatus.ForeColor = Color.FromArgb(231, 76, 60);
            lblStatus.TextAlign = ContentAlignment.MiddleCenter;
            Controls.Add(lblStatus);
            y += 24;

            // 5. Tombol Aksi
            btnAktivasi.Text = "✔ Aktivasi Sekarang";
            btnAktivasi.Location = new Point(x, y);
            btnAktivasi.Size = new Size(350, 42);
            btnAktivasi.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnAktivasi.BackColor = Color.FromArgb(39, 174, 96);
            btnAktivasi.ForeColor = Color.White;
            btnAktivasi.FlatStyle = FlatStyle.Flat;
            btnAktivasi.FlatAppearance.BorderSize = 0;
            btnAktivasi.Cursor = Cursors.Hand;
            btnAktivasi.Click += BtnAktivasi_Click;
            Controls.Add(btnAktivasi);

            btnKeluar.Text = "✖ Keluar";
            btnKeluar.Location = new Point(x + 364, y);
            btnKeluar.Size = new Size(160, 42);
            btnKeluar.Font = new Font("Segoe UI", 10.5F);
            btnKeluar.BackColor = Color.FromArgb(149, 165, 166);
            btnKeluar.ForeColor = Color.White;
            btnKeluar.FlatStyle = FlatStyle.Flat;
            btnKeluar.FlatAppearance.BorderSize = 0;
            btnKeluar.Cursor = Cursors.Hand;
            btnKeluar.Click += (_, _) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };
            Controls.Add(btnKeluar);
        }

        private void BtnAktivasi_Click(object? sender, EventArgs e)
        {
            string key = txtKunciAktivasi.Text.Trim();
            if (string.IsNullOrWhiteSpace(key))
            {
                lblStatus.Text = "Silakan masukkan kunci aktivasi terlebih dahulu!";
                txtKunciAktivasi.Focus();
                return;
            }

            btnAktivasi.Enabled = false;
            btnAktivasi.Text = "Memverifikasi...";

            if (LicenseManager.Aktivasi(key, out string pesan))
            {
                MessageBox.Show(
                    pesan + "\n\nTerima kasih, selamat menggunakan Sistem POS SMK Negeri 4!",
                    "Aktivasi Sukses",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                lblStatus.Text = pesan;
                MessageBox.Show(pesan, "Aktivasi Gagal", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnAktivasi.Enabled = true;
                btnAktivasi.Text = "✔ Aktivasi Sekarang";
                txtKunciAktivasi.Focus();
                txtKunciAktivasi.SelectAll();
            }
        }
    }
}
