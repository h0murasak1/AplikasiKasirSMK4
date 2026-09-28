namespace AplikasiKasirSMK4
{
    partial class FormMenu
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            panelTop = new Panel();
            panelNavBar = new FlowLayoutPanel();
            navKasir = new Button();
            navBarang = new Button();
            navLaporan = new Button();
            navStok = new Button();
            navPiutang = new Button();
            navKomisi = new Button();
            navMaster = new Button();
            navBackup = new Button();
            panelBrand = new Panel();
            pnlBrandKanan = new FlowLayoutPanel();
            lblUserStatus = new Label();
            btnTopLogout = new Button();
            picBrandLogo = new PictureBox();
            lblBrand = new Label();
            panelKonten = new Panel();
            panelTop.SuspendLayout();
            panelNavBar.SuspendLayout();
            panelBrand.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picBrandLogo).BeginInit();
            pnlBrandKanan.SuspendLayout();
            SuspendLayout();
            // 
            // panelTop
            // 
            panelTop.Controls.Add(panelNavBar);
            panelTop.Controls.Add(panelBrand);
            panelTop.Dock = DockStyle.Top;
            panelTop.Location = new Point(0, 0);
            panelTop.Name = "panelTop";
            panelTop.Size = new Size(1200, 108);
            panelTop.TabIndex = 0;
            // 
            // panelBrand
            // 
            panelBrand.BackColor = Color.FromArgb(15, 23, 42);
            panelBrand.Controls.Add(picBrandLogo);
            panelBrand.Controls.Add(pnlBrandKanan);
            panelBrand.Controls.Add(lblBrand);
            panelBrand.Dock = DockStyle.Top;
            panelBrand.Location = new Point(0, 0);
            panelBrand.Name = "panelBrand";
            panelBrand.Padding = new Padding(16, 0, 16, 0);
            panelBrand.Size = new Size(1200, 52);
            panelBrand.TabIndex = 0;
            // 
            // picBrandLogo
            // 
            picBrandLogo.Location = new Point(14, 8);
            picBrandLogo.Name = "picBrandLogo";
            picBrandLogo.Size = new Size(36, 36);
            picBrandLogo.SizeMode = PictureBoxSizeMode.Zoom;
            picBrandLogo.TabIndex = 2;
            picBrandLogo.TabStop = false;
            // 
            // lblBrand
            // 
            lblBrand.AutoSize = true;
            lblBrand.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblBrand.ForeColor = Color.White;
            lblBrand.Location = new Point(58, 12);
            lblBrand.Name = "lblBrand";
            lblBrand.Size = new Size(330, 28);
            lblBrand.TabIndex = 0;
            lblBrand.Text = "SISTEM POS SMK NEGERI 4 TANGERANG";
            // 
            // pnlBrandKanan
            // 
            pnlBrandKanan.AutoSize = true;
            pnlBrandKanan.BackColor = Color.Transparent;
            pnlBrandKanan.Controls.Add(lblUserStatus);
            pnlBrandKanan.Controls.Add(btnTopLogout);
            pnlBrandKanan.Dock = DockStyle.Right;
            pnlBrandKanan.FlowDirection = FlowDirection.LeftToRight;
            pnlBrandKanan.Location = new Point(880, 0);
            pnlBrandKanan.Name = "pnlBrandKanan";
            pnlBrandKanan.Padding = new Padding(0, 8, 0, 0);
            pnlBrandKanan.Size = new Size(304, 52);
            pnlBrandKanan.TabIndex = 1;
            pnlBrandKanan.WrapContents = false;
            // 
            // lblUserStatus
            // 
            lblUserStatus.AutoSize = true;
            lblUserStatus.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblUserStatus.ForeColor = Color.FromArgb(148, 163, 184);
            lblUserStatus.Location = new Point(0, 6);
            lblUserStatus.Margin = new Padding(0, 6, 14, 0);
            lblUserStatus.Name = "lblUserStatus";
            lblUserStatus.Size = new Size(185, 21);
            lblUserStatus.TabIndex = 0;
            lblUserStatus.Text = "👤 Administrator";
            // 
            // btnTopLogout
            // 
            btnTopLogout.BackColor = Color.FromArgb(239, 68, 68);
            btnTopLogout.Cursor = Cursors.Hand;
            btnTopLogout.FlatAppearance.BorderSize = 0;
            btnTopLogout.FlatStyle = FlatStyle.Flat;
            btnTopLogout.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnTopLogout.ForeColor = Color.White;
            btnTopLogout.Location = new Point(199, 0);
            btnTopLogout.Margin = new Padding(0);
            btnTopLogout.Name = "btnTopLogout";
            btnTopLogout.Size = new Size(100, 36);
            btnTopLogout.TabIndex = 1;
            btnTopLogout.Text = "🚪 Logout";
            btnTopLogout.UseVisualStyleBackColor = false;
            btnTopLogout.Click += btnLogout_Click;
            // 
            // panelNavBar
            // 
            panelNavBar.BackColor = Color.FromArgb(30, 41, 59);
            panelNavBar.Controls.Add(navKasir);
            panelNavBar.Controls.Add(navBarang);
            panelNavBar.Controls.Add(navLaporan);
            panelNavBar.Controls.Add(navStok);
            panelNavBar.Controls.Add(navPiutang);
            panelNavBar.Controls.Add(navKomisi);
            panelNavBar.Controls.Add(navMaster);
            panelNavBar.Controls.Add(navBackup);
            panelNavBar.Dock = DockStyle.Fill;
            panelNavBar.FlowDirection = FlowDirection.LeftToRight;
            panelNavBar.Location = new Point(0, 52);
            panelNavBar.Name = "panelNavBar";
            panelNavBar.Padding = new Padding(12, 6, 12, 0);
            panelNavBar.Size = new Size(1200, 56);
            panelNavBar.TabIndex = 1;
            panelNavBar.WrapContents = false;
            // 
            // navKasir
            // 
            navKasir.BackColor = Color.FromArgb(37, 99, 235);
            navKasir.Cursor = Cursors.Hand;
            navKasir.FlatAppearance.BorderSize = 0;
            navKasir.FlatStyle = FlatStyle.Flat;
            navKasir.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            navKasir.ForeColor = Color.White;
            navKasir.Location = new Point(16, 8);
            navKasir.Margin = new Padding(4, 2, 4, 2);
            navKasir.Name = "navKasir";
            navKasir.Size = new Size(130, 42);
            navKasir.TabIndex = 0;
            navKasir.Text = "🛒 KASIR";
            navKasir.UseVisualStyleBackColor = false;
            navKasir.Click += navKasir_Click;
            // 
            // navBarang
            // 
            navBarang.BackColor = Color.FromArgb(51, 65, 85);
            navBarang.Cursor = Cursors.Hand;
            navBarang.FlatAppearance.BorderSize = 0;
            navBarang.FlatStyle = FlatStyle.Flat;
            navBarang.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            navBarang.ForeColor = Color.FromArgb(203, 213, 225);
            navBarang.Location = new Point(154, 8);
            navBarang.Margin = new Padding(4, 2, 4, 2);
            navBarang.Name = "navBarang";
            navBarang.Size = new Size(130, 42);
            navBarang.TabIndex = 1;
            navBarang.Text = "📦 BARANG";
            navBarang.UseVisualStyleBackColor = false;
            navBarang.Click += navBarang_Click;
            // 
            // navLaporan
            // 
            navLaporan.BackColor = Color.FromArgb(51, 65, 85);
            navLaporan.Cursor = Cursors.Hand;
            navLaporan.FlatAppearance.BorderSize = 0;
            navLaporan.FlatStyle = FlatStyle.Flat;
            navLaporan.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            navLaporan.ForeColor = Color.FromArgb(203, 213, 225);
            navLaporan.Location = new Point(292, 8);
            navLaporan.Margin = new Padding(4, 2, 4, 2);
            navLaporan.Name = "navLaporan";
            navLaporan.Size = new Size(140, 42);
            navLaporan.TabIndex = 2;
            navLaporan.Text = "📊 LAPORAN";
            navLaporan.UseVisualStyleBackColor = false;
            navLaporan.Click += navLaporan_Click;
            // 
            // navStok
            // 
            navStok.BackColor = Color.FromArgb(51, 65, 85);
            navStok.Cursor = Cursors.Hand;
            navStok.FlatAppearance.BorderSize = 0;
            navStok.FlatStyle = FlatStyle.Flat;
            navStok.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            navStok.ForeColor = Color.FromArgb(203, 213, 225);
            navStok.Location = new Point(440, 8);
            navStok.Margin = new Padding(4, 2, 4, 2);
            navStok.Name = "navStok";
            navStok.Size = new Size(130, 42);
            navStok.TabIndex = 3;
            navStok.Text = "📋 STOK";
            navStok.UseVisualStyleBackColor = false;
            navStok.Click += navStok_Click;
            // 
            // navPiutang
            // 
            navPiutang.BackColor = Color.FromArgb(51, 65, 85);
            navPiutang.Cursor = Cursors.Hand;
            navPiutang.FlatAppearance.BorderSize = 0;
            navPiutang.FlatStyle = FlatStyle.Flat;
            navPiutang.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            navPiutang.ForeColor = Color.FromArgb(203, 213, 225);
            navPiutang.Location = new Point(578, 8);
            navPiutang.Margin = new Padding(4, 2, 4, 2);
            navPiutang.Name = "navPiutang";
            navPiutang.Size = new Size(130, 42);
            navPiutang.TabIndex = 4;
            navPiutang.Text = "💳 PIUTANG";
            navPiutang.UseVisualStyleBackColor = false;
            navPiutang.Click += navPiutang_Click;
            // 
            // navKomisi
            // 
            navKomisi.BackColor = Color.FromArgb(51, 65, 85);
            navKomisi.Cursor = Cursors.Hand;
            navKomisi.FlatAppearance.BorderSize = 0;
            navKomisi.FlatStyle = FlatStyle.Flat;
            navKomisi.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            navKomisi.ForeColor = Color.FromArgb(203, 213, 225);
            navKomisi.Location = new Point(716, 8);
            navKomisi.Margin = new Padding(4, 2, 4, 2);
            navKomisi.Name = "navKomisi";
            navKomisi.Size = new Size(130, 42);
            navKomisi.TabIndex = 5;
            navKomisi.Text = "👔 KOMISI";
            navKomisi.UseVisualStyleBackColor = false;
            navKomisi.Click += navKomisi_Click;
            // 
            // navMaster
            // 
            navMaster.BackColor = Color.FromArgb(51, 65, 85);
            navMaster.Cursor = Cursors.Hand;
            navMaster.FlatAppearance.BorderSize = 0;
            navMaster.FlatStyle = FlatStyle.Flat;
            navMaster.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            navMaster.ForeColor = Color.FromArgb(203, 213, 225);
            navMaster.Location = new Point(854, 8);
            navMaster.Margin = new Padding(4, 2, 4, 2);
            navMaster.Name = "navMaster";
            navMaster.Size = new Size(140, 42);
            navMaster.TabIndex = 6;
            navMaster.Text = "⚙️ MASTER";
            navMaster.UseVisualStyleBackColor = false;
            navMaster.Click += navMaster_Click;
            // 
            // navBackup
            // 
            navBackup.BackColor = Color.FromArgb(51, 65, 85);
            navBackup.Cursor = Cursors.Hand;
            navBackup.FlatAppearance.BorderSize = 0;
            navBackup.FlatStyle = FlatStyle.Flat;
            navBackup.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            navBackup.ForeColor = Color.FromArgb(203, 213, 225);
            navBackup.Location = new Point(1002, 8);
            navBackup.Margin = new Padding(4, 2, 4, 2);
            navBackup.Name = "navBackup";
            navBackup.Size = new Size(140, 42);
            navBackup.TabIndex = 7;
            navBackup.Text = "💾 BACKUP";
            navBackup.UseVisualStyleBackColor = false;
            navBackup.Click += navBackup_Click;
            // 
            // panelKonten
            // 
            panelKonten.BackColor = Color.FromArgb(248, 250, 252);
            panelKonten.Dock = DockStyle.Fill;
            panelKonten.Location = new Point(0, 108);
            panelKonten.Name = "panelKonten";
            panelKonten.Size = new Size(1200, 612);
            panelKonten.TabIndex = 1;
            // 
            // FormMenu
            // 
            BackColor = Color.FromArgb(248, 250, 252);
            ClientSize = new Size(1200, 720);
            Controls.Add(panelKonten);
            Controls.Add(panelTop);
            MinimumSize = new Size(1024, 680);
            Name = "FormMenu";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Dashboard Utama - Sistem POS SMK Negeri 4";
            WindowState = FormWindowState.Maximized;
            panelTop.ResumeLayout(false);
            panelNavBar.ResumeLayout(false);
            panelBrand.ResumeLayout(false);
            panelBrand.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picBrandLogo).EndInit();
            pnlBrandKanan.ResumeLayout(false);
            pnlBrandKanan.PerformLayout();
            ResumeLayout(false);
        }

        private Panel panelTop;
        private Panel panelBrand;
        private PictureBox picBrandLogo;
        private Label lblBrand;
        private FlowLayoutPanel pnlBrandKanan;
        private Label lblUserStatus;
        private Button btnTopLogout;
        private FlowLayoutPanel panelNavBar;
        private Button navKasir;
        private Button navBarang;
        private Button navLaporan;
        private Button navStok;
        private Button navPiutang;
        private Button navKomisi;
        private Button navMaster;
        private Button navBackup;
        private Panel panelKonten;
    }
}