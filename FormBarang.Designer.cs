namespace AplikasiKasirSMK4
{
    partial class FormBarang
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

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.panelHeader = new System.Windows.Forms.Panel();
            this.lblJudul = new System.Windows.Forms.Label();
            this.panelKiri = new System.Windows.Forms.Panel();
            this.lblKode = new System.Windows.Forms.Label();
            this.txtKode = new ReaLTaiizor.Controls.HopeTextBox();
            this.lblNama = new System.Windows.Forms.Label();
            this.txtNama = new ReaLTaiizor.Controls.HopeTextBox();
            this.lblHargaBeli = new System.Windows.Forms.Label();
            this.txtHargaBeli = new ReaLTaiizor.Controls.HopeTextBox();
            this.lblHargaJual = new System.Windows.Forms.Label();
            this.txtHargaJual = new ReaLTaiizor.Controls.HopeTextBox();
            this.lblStok = new System.Windows.Forms.Label();
            this.txtStok = new ReaLTaiizor.Controls.HopeTextBox();
            this.btnSimpan = new ReaLTaiizor.Controls.HopeButton();
            this.btnEdit = new ReaLTaiizor.Controls.HopeButton();
            this.btnHapus = new ReaLTaiizor.Controls.HopeButton();
            this.btnBersihkan = new ReaLTaiizor.Controls.HopeButton();
            this.panelKanan = new System.Windows.Forms.Panel();
            this.dgvBarang = new System.Windows.Forms.DataGridView();

            this.panelHeader.SuspendLayout();
            this.panelKiri.SuspendLayout();
            this.panelKanan.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvBarang)).BeginInit();
            this.SuspendLayout();

            // panelHeader
            this.panelHeader.BackColor = System.Drawing.Color.FromArgb(41, 128, 185);
            this.panelHeader.Controls.Add(this.lblJudul);
            this.panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHeader.Height = 70;

            // lblJudul
            this.lblJudul.AutoSize = true;
            this.lblJudul.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblJudul.ForeColor = System.Drawing.Color.White;
            this.lblJudul.Location = new System.Drawing.Point(20, 18);
            this.lblJudul.Text = "KELOLA DATA BARANG - GUDANG";

            // panelKiri
            this.panelKiri.BackColor = System.Drawing.Color.WhiteSmoke;
            this.panelKiri.Controls.Add(this.lblKode);
            this.panelKiri.Controls.Add(this.txtKode);
            this.panelKiri.Controls.Add(this.lblNama);
            this.panelKiri.Controls.Add(this.txtNama);
            this.panelKiri.Controls.Add(this.lblHargaBeli);
            this.panelKiri.Controls.Add(this.txtHargaBeli);
            this.panelKiri.Controls.Add(this.lblHargaJual);
            this.panelKiri.Controls.Add(this.txtHargaJual);
            this.panelKiri.Controls.Add(this.lblStok);
            this.panelKiri.Controls.Add(this.txtStok);
            this.panelKiri.Controls.Add(this.btnSimpan);
            this.panelKiri.Controls.Add(this.btnEdit);
            this.panelKiri.Controls.Add(this.btnHapus);
            this.panelKiri.Controls.Add(this.btnBersihkan);
            this.panelKiri.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelKiri.Width = 350;
            this.panelKiri.Padding = new System.Windows.Forms.Padding(20);

            this.lblSatuan = new System.Windows.Forms.Label();
            this.cmbSatuan = new System.Windows.Forms.ComboBox();
            this.panelKiri.Controls.Add(this.lblSatuan);
            this.panelKiri.Controls.Add(this.cmbSatuan);

            // lblKode & txtKode
            this.lblKode.AutoSize = true;
            this.lblKode.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblKode.Location = new System.Drawing.Point(20, 20);
            this.lblKode.Text = "Kode Barcode";
            this.txtKode.BackColor = System.Drawing.Color.White;
            this.txtKode.BaseColor = System.Drawing.Color.White;
            this.txtKode.BorderColorA = System.Drawing.Color.FromArgb(41, 128, 185);
            this.txtKode.BorderColorB = System.Drawing.Color.Silver;
            this.txtKode.ForeColor = System.Drawing.Color.FromArgb(48, 49, 51);
            this.txtKode.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.txtKode.Location = new System.Drawing.Point(20, 45);
            this.txtKode.Width = 310;
            this.txtKode.Height = 38;

            // lblNama & txtNama
            this.lblNama.AutoSize = true;
            this.lblNama.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblNama.Location = new System.Drawing.Point(20, 90);
            this.lblNama.Text = "Nama Barang";
            this.txtNama.BackColor = System.Drawing.Color.White;
            this.txtNama.BaseColor = System.Drawing.Color.White;
            this.txtNama.BorderColorA = System.Drawing.Color.FromArgb(41, 128, 185);
            this.txtNama.BorderColorB = System.Drawing.Color.Silver;
            this.txtNama.ForeColor = System.Drawing.Color.FromArgb(48, 49, 51);
            this.txtNama.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.txtNama.Location = new System.Drawing.Point(20, 115);
            this.txtNama.Width = 310;
            this.txtNama.Height = 38;

            // lblHargaBeli & txtHargaBeli
            this.lblHargaBeli.AutoSize = true;
            this.lblHargaBeli.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblHargaBeli.Location = new System.Drawing.Point(20, 160);
            this.lblHargaBeli.Text = "Harga Beli (Rp)";
            this.txtHargaBeli.BackColor = System.Drawing.Color.White;
            this.txtHargaBeli.BaseColor = System.Drawing.Color.White;
            this.txtHargaBeli.BorderColorA = System.Drawing.Color.FromArgb(41, 128, 185);
            this.txtHargaBeli.BorderColorB = System.Drawing.Color.Silver;
            this.txtHargaBeli.ForeColor = System.Drawing.Color.FromArgb(48, 49, 51);
            this.txtHargaBeli.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.txtHargaBeli.Location = new System.Drawing.Point(20, 185);
            this.txtHargaBeli.Width = 310;
            this.txtHargaBeli.Height = 38;

            // lblHargaJual & txtHargaJual
            this.lblHargaJual.AutoSize = true;
            this.lblHargaJual.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblHargaJual.Location = new System.Drawing.Point(20, 230);
            this.lblHargaJual.Text = "Harga Jual (Rp)";
            this.txtHargaJual.BackColor = System.Drawing.Color.White;
            this.txtHargaJual.BaseColor = System.Drawing.Color.White;
            this.txtHargaJual.BorderColorA = System.Drawing.Color.FromArgb(41, 128, 185);
            this.txtHargaJual.BorderColorB = System.Drawing.Color.Silver;
            this.txtHargaJual.ForeColor = System.Drawing.Color.FromArgb(48, 49, 51);
            this.txtHargaJual.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.txtHargaJual.Location = new System.Drawing.Point(20, 255);
            this.txtHargaJual.Width = 310;
            this.txtHargaJual.Height = 38;

            // lblStok & txtStok
            this.lblStok.AutoSize = true;
            this.lblStok.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblStok.Location = new System.Drawing.Point(20, 300);
            this.lblStok.Text = "Jumlah Stok";
            this.txtStok.BackColor = System.Drawing.Color.White;
            this.txtStok.BaseColor = System.Drawing.Color.White;
            this.txtStok.BorderColorA = System.Drawing.Color.FromArgb(41, 128, 185);
            this.txtStok.BorderColorB = System.Drawing.Color.Silver;
            this.txtStok.ForeColor = System.Drawing.Color.FromArgb(48, 49, 51);
            this.txtStok.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.txtStok.Location = new System.Drawing.Point(20, 325);
            this.txtStok.Width = 145;
            this.txtStok.Height = 38;

            // lblSatuan & cmbSatuan
            this.lblSatuan.AutoSize = true;
            this.lblSatuan.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblSatuan.Location = new System.Drawing.Point(180, 300);
            this.lblSatuan.Text = "Satuan";
            this.cmbSatuan.BackColor = System.Drawing.Color.White;
            this.cmbSatuan.ForeColor = System.Drawing.Color.FromArgb(48, 49, 51);
            this.cmbSatuan.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.cmbSatuan.Location = new System.Drawing.Point(180, 328);
            this.cmbSatuan.Width = 150;
            this.cmbSatuan.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.cmbSatuan.Items.AddRange(new object[] {
                "pcs", "kg", "gram", "liter", "box", "lusin", "pack", "botol", "bungkus", "porsi", "lembar"
            });

            // Tombol-Tombol
            this.btnSimpan.Location = new System.Drawing.Point(20, 390);
            this.btnSimpan.Width = 150;
            this.btnSimpan.Height = 40;
            this.btnSimpan.Text = "SIMPAN";
            this.btnSimpan.PrimaryColor = System.Drawing.Color.FromArgb(39, 174, 96);
            this.btnSimpan.Click += new System.EventHandler(this.btnSimpan_Click);

            this.btnEdit.Location = new System.Drawing.Point(180, 390);
            this.btnEdit.Width = 150;
            this.btnEdit.Height = 40;
            this.btnEdit.Text = "PERBARUI";
            this.btnEdit.PrimaryColor = System.Drawing.Color.FromArgb(243, 156, 18);
            this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);

            this.btnHapus.Location = new System.Drawing.Point(20, 440);
            this.btnHapus.Width = 150;
            this.btnHapus.Height = 40;
            this.btnHapus.Text = "HAPUS";
            this.btnHapus.PrimaryColor = System.Drawing.Color.FromArgb(231, 76, 60);
            this.btnHapus.Click += new System.EventHandler(this.btnHapus_Click);

            this.btnBersihkan.Location = new System.Drawing.Point(180, 440);
            this.btnBersihkan.Width = 150;
            this.btnBersihkan.Height = 40;
            this.btnBersihkan.Text = "BERSIHKAN";
            this.btnBersihkan.PrimaryColor = System.Drawing.Color.FromArgb(149, 165, 166);
            this.btnBersihkan.Click += new System.EventHandler(this.btnBersihkan_Click);

            // panelKanan
            this.panelKanan.BackColor = System.Drawing.Color.White;
            this.panelKanan.Controls.Add(this.dgvBarang);
            this.panelKanan.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelKanan.Padding = new System.Windows.Forms.Padding(20);

            // dgvBarang
            this.dgvBarang.BackgroundColor = System.Drawing.Color.White;
            this.dgvBarang.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvBarang.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvBarang.AllowUserToAddRows = false;
            this.dgvBarang.AllowUserToDeleteRows = false;
            this.dgvBarang.ReadOnly = true;
            this.dgvBarang.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvBarang.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.dgvBarang.RowTemplate.Height = 35;
            this.dgvBarang.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvBarang.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvBarang_CellClick);

            // FormBarang
            this.ClientSize = new System.Drawing.Size(1000, 600);
            this.Controls.Add(this.panelKanan);
            this.Controls.Add(this.panelKiri);
            this.Controls.Add(this.panelHeader);
            this.Name = "FormBarang";
            this.Text = "Master Barang - SMK Negeri 4";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.FormBarang_Load);

            this.panelHeader.ResumeLayout(false);
            this.panelHeader.PerformLayout();
            this.panelKiri.ResumeLayout(false);
            this.panelKiri.PerformLayout();
            this.panelKanan.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvBarang)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.Label lblJudul;
        private System.Windows.Forms.Panel panelKiri;
        private System.Windows.Forms.Label lblKode;
        private ReaLTaiizor.Controls.HopeTextBox txtKode;
        private System.Windows.Forms.Label lblNama;
        private ReaLTaiizor.Controls.HopeTextBox txtNama;
        private System.Windows.Forms.Label lblHargaBeli;
        private ReaLTaiizor.Controls.HopeTextBox txtHargaBeli;
        private System.Windows.Forms.Label lblHargaJual;
        private ReaLTaiizor.Controls.HopeTextBox txtHargaJual;
        private System.Windows.Forms.Label lblStok;
        private ReaLTaiizor.Controls.HopeTextBox txtStok;
        private System.Windows.Forms.Label lblSatuan;
        private System.Windows.Forms.ComboBox cmbSatuan;
        private ReaLTaiizor.Controls.HopeButton btnSimpan;
        private ReaLTaiizor.Controls.HopeButton btnEdit;
        private ReaLTaiizor.Controls.HopeButton btnHapus;
        private ReaLTaiizor.Controls.HopeButton btnBersihkan;
        private System.Windows.Forms.Panel panelKanan;
        private System.Windows.Forms.DataGridView dgvBarang;
    }
}