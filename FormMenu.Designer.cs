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
            this.panelHeader = new System.Windows.Forms.Panel();
            this.lblJudul = new System.Windows.Forms.Label();
            this.btnInputBarang = new ReaLTaiizor.Controls.HopeButton();
            this.btnKasir = new ReaLTaiizor.Controls.HopeButton();
            this.btnLaporan = new ReaLTaiizor.Controls.HopeButton();
            this.btnLogout = new ReaLTaiizor.Controls.HopeButton();
            this.panelHeader.SuspendLayout();
            this.SuspendLayout();

            // panelHeader
            this.panelHeader.BackColor = System.Drawing.Color.FromArgb(41, 128, 185);
            this.panelHeader.Controls.Add(this.lblJudul);
            this.panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHeader.Height = 80;

            // lblJudul
            this.lblJudul.AutoSize = true;
            this.lblJudul.Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Bold);
            this.lblJudul.ForeColor = System.Drawing.Color.White;
            this.lblJudul.Location = new System.Drawing.Point(20, 20);
            this.lblJudul.Text = "DASHBOARD ADMINISTRATOR";

            // btnInputBarang (Tombol Input Barang)
            this.btnInputBarang.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnInputBarang.Location = new System.Drawing.Point(40, 120);
            this.btnInputBarang.PrimaryColor = System.Drawing.Color.FromArgb(39, 174, 96);
            this.btnInputBarang.Size = new System.Drawing.Size(220, 80);
            this.btnInputBarang.Text = "INPUT DATA BARANG";
            this.btnInputBarang.Click += new System.EventHandler(this.btnInputBarang_Click);

            // btnKasir
            this.btnKasir.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnKasir.Location = new System.Drawing.Point(280, 120);
            this.btnKasir.PrimaryColor = System.Drawing.Color.FromArgb(243, 156, 18);
            this.btnKasir.Size = new System.Drawing.Size(220, 80);
            this.btnKasir.Text = "BUKA MESIN KASIR";
            this.btnKasir.Click += new System.EventHandler(this.btnKasir_Click);

            // btnLaporan
            this.btnLaporan.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnLaporan.Location = new System.Drawing.Point(520, 120);
            this.btnLaporan.PrimaryColor = System.Drawing.Color.FromArgb(41, 128, 185);
            this.btnLaporan.Size = new System.Drawing.Size(220, 80);
            this.btnLaporan.Text = "LAPORAN PENJUALAN";
            this.btnLaporan.Click += new System.EventHandler(this.btnLaporan_Click);

            // btnLogout
            this.btnLogout.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnLogout.Location = new System.Drawing.Point(40, 230);
            this.btnLogout.PrimaryColor = System.Drawing.Color.FromArgb(231, 76, 60);
            this.btnLogout.Size = new System.Drawing.Size(700, 50);
            this.btnLogout.Text = "LOGOUT (KELUAR)";
            this.btnLogout.Click += new System.EventHandler(this.btnLogout_Click);

            // FormMenu
            this.ClientSize = new System.Drawing.Size(780, 320);
            this.Controls.Add(this.btnLogout);
            this.Controls.Add(this.btnLaporan);
            this.Controls.Add(this.btnKasir);
            this.Controls.Add(this.btnInputBarang);
            this.Controls.Add(this.panelHeader);
            this.Name = "FormMenu";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Text = "Menu Utama - SMK Negeri 4";
            this.panelHeader.ResumeLayout(false);
            this.panelHeader.PerformLayout();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.Label lblJudul;
        private ReaLTaiizor.Controls.HopeButton btnInputBarang;
        private ReaLTaiizor.Controls.HopeButton btnKasir;
        private ReaLTaiizor.Controls.HopeButton btnLaporan;
        private ReaLTaiizor.Controls.HopeButton btnLogout;
    }
}