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
            this.lblJenis = new System.Windows.Forms.Label();
            this.cmbJenis = new System.Windows.Forms.ComboBox();
            this.lblMerek = new System.Windows.Forms.Label();
            this.cmbMerek = new System.Windows.Forms.ComboBox();
            this.lblSupplier = new System.Windows.Forms.Label();
            this.cmbSupplier = new System.Windows.Forms.ComboBox();
            this.lblHargaBeli = new System.Windows.Forms.Label();
            this.txtHargaBeli = new ReaLTaiizor.Controls.HopeTextBox();
            this.lblHargaJual = new System.Windows.Forms.Label();
            this.txtHargaJual = new ReaLTaiizor.Controls.HopeTextBox();
            this.lblMinGrosir = new System.Windows.Forms.Label();
            this.txtMinGrosir = new ReaLTaiizor.Controls.HopeTextBox();
            this.lblHargaGrosir = new System.Windows.Forms.Label();
            this.txtHargaGrosir = new ReaLTaiizor.Controls.HopeTextBox();
            this.lblStok = new System.Windows.Forms.Label();
            this.txtStok = new ReaLTaiizor.Controls.HopeTextBox();
            this.lblSatuan = new System.Windows.Forms.Label();
            this.cmbSatuan = new System.Windows.Forms.ComboBox();
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
            this.panelKiri.AutoScroll = true;
            this.panelKiri.Controls.Add(this.lblKode);
            this.panelKiri.Controls.Add(this.txtKode);
            this.panelKiri.Controls.Add(this.lblNama);
            this.panelKiri.Controls.Add(this.txtNama);
            this.panelKiri.Controls.Add(this.lblJenis);
            this.panelKiri.Controls.Add(this.cmbJenis);
            this.panelKiri.Controls.Add(this.lblMerek);
            this.panelKiri.Controls.Add(this.cmbMerek);
            this.panelKiri.Controls.Add(this.lblSupplier);
            this.panelKiri.Controls.Add(this.cmbSupplier);
            this.panelKiri.Controls.Add(this.lblHargaBeli);
            this.panelKiri.Controls.Add(this.txtHargaBeli);
            this.panelKiri.Controls.Add(this.lblHargaJual);
            this.panelKiri.Controls.Add(this.txtHargaJual);
            this.panelKiri.Controls.Add(this.lblMinGrosir);
            this.panelKiri.Controls.Add(this.txtMinGrosir);
            this.panelKiri.Controls.Add(this.lblHargaGrosir);
            this.panelKiri.Controls.Add(this.txtHargaGrosir);
            this.panelKiri.Controls.Add(this.lblStok);
            this.panelKiri.Controls.Add(this.txtStok);
            this.panelKiri.Controls.Add(this.lblSatuan);
            this.panelKiri.Controls.Add(this.cmbSatuan);
            this.panelKiri.Controls.Add(this.btnSimpan);
            this.panelKiri.Controls.Add(this.btnEdit);
            this.panelKiri.Controls.Add(this.btnHapus);
            this.panelKiri.Controls.Add(this.btnBersihkan);
            this.panelKiri.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelKiri.Width = 380;
            this.panelKiri.Padding = new System.Windows.Forms.Padding(16);

            // lblKode & txtKode
            this.lblKode.AutoSize = true;
            this.lblKode.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblKode.Location = new System.Drawing.Point(16, 10);
            this.lblKode.Text = "Kode Barcode *";
            this.txtKode.BackColor = System.Drawing.Color.White;
            this.txtKode.BaseColor = System.Drawing.Color.White;
            this.txtKode.BorderColorA = System.Drawing.Color.FromArgb(41, 128, 185);
            this.txtKode.BorderColorB = System.Drawing.Color.Silver;
            this.txtKode.ForeColor = System.Drawing.Color.FromArgb(48, 49, 51);
            this.txtKode.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.txtKode.Location = new System.Drawing.Point(16, 30);
            this.txtKode.Width = 330;
            this.txtKode.Height = 36;

            // lblNama & txtNama
            this.lblNama.AutoSize = true;
            this.lblNama.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblNama.Location = new System.Drawing.Point(16, 72);
            this.lblNama.Text = "Nama Barang *";
            this.txtNama.BackColor = System.Drawing.Color.White;
            this.txtNama.BaseColor = System.Drawing.Color.White;
            this.txtNama.BorderColorA = System.Drawing.Color.FromArgb(41, 128, 185);
            this.txtNama.BorderColorB = System.Drawing.Color.Silver;
            this.txtNama.ForeColor = System.Drawing.Color.FromArgb(48, 49, 51);
            this.txtNama.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.txtNama.Location = new System.Drawing.Point(16, 92);
            this.txtNama.Width = 330;
            this.txtNama.Height = 36;

            // Jenis, Merek, Supplier
            this.lblJenis.AutoSize = true;
            this.lblJenis.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblJenis.Location = new System.Drawing.Point(16, 134);
            this.lblJenis.Text = "Jenis Barang:";
            this.cmbJenis.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.cmbJenis.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbJenis.Location = new System.Drawing.Point(16, 154);
            this.cmbJenis.Width = 160;

            this.lblMerek.AutoSize = true;
            this.lblMerek.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblMerek.Location = new System.Drawing.Point(186, 134);
            this.lblMerek.Text = "Merek Barang:";
            this.cmbMerek.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.cmbMerek.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbMerek.Location = new System.Drawing.Point(186, 154);
            this.cmbMerek.Width = 160;

            this.lblSupplier.AutoSize = true;
            this.lblSupplier.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblSupplier.Location = new System.Drawing.Point(16, 188);
            this.lblSupplier.Text = "Supplier:";
            this.cmbSupplier.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.cmbSupplier.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSupplier.Location = new System.Drawing.Point(16, 208);
            this.cmbSupplier.Width = 330;

            // Harga Beli & Harga Jual
            this.lblHargaBeli.AutoSize = true;
            this.lblHargaBeli.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblHargaBeli.Location = new System.Drawing.Point(16, 242);
            this.lblHargaBeli.Text = "Harga Beli (Rp)";
            this.txtHargaBeli.BackColor = System.Drawing.Color.White;
            this.txtHargaBeli.BaseColor = System.Drawing.Color.White;
            this.txtHargaBeli.BorderColorA = System.Drawing.Color.FromArgb(41, 128, 185);
            this.txtHargaBeli.BorderColorB = System.Drawing.Color.Silver;
            this.txtHargaBeli.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            this.txtHargaBeli.Location = new System.Drawing.Point(16, 262);
            this.txtHargaBeli.Width = 160;
            this.txtHargaBeli.Height = 36;

            this.lblHargaJual.AutoSize = true;
            this.lblHargaJual.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblHargaJual.Location = new System.Drawing.Point(186, 242);
            this.lblHargaJual.Text = "Harga Jual (Rp) *";
            this.txtHargaJual.BackColor = System.Drawing.Color.White;
            this.txtHargaJual.BaseColor = System.Drawing.Color.White;
            this.txtHargaJual.BorderColorA = System.Drawing.Color.FromArgb(41, 128, 185);
            this.txtHargaJual.BorderColorB = System.Drawing.Color.Silver;
            this.txtHargaJual.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            this.txtHargaJual.Location = new System.Drawing.Point(186, 262);
            this.txtHargaJual.Width = 160;
            this.txtHargaJual.Height = 36;

            // Min. Grosir & Harga Grosir
            this.lblMinGrosir.AutoSize = true;
            this.lblMinGrosir.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblMinGrosir.Location = new System.Drawing.Point(16, 304);
            this.lblMinGrosir.Text = "Min. Grosir (Qty)";
            this.txtMinGrosir.BackColor = System.Drawing.Color.White;
            this.txtMinGrosir.BaseColor = System.Drawing.Color.White;
            this.txtMinGrosir.BorderColorA = System.Drawing.Color.FromArgb(41, 128, 185);
            this.txtMinGrosir.BorderColorB = System.Drawing.Color.Silver;
            this.txtMinGrosir.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            this.txtMinGrosir.Location = new System.Drawing.Point(16, 324);
            this.txtMinGrosir.Width = 160;
            this.txtMinGrosir.Height = 36;

            this.lblHargaGrosir.AutoSize = true;
            this.lblHargaGrosir.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblHargaGrosir.Location = new System.Drawing.Point(186, 304);
            this.lblHargaGrosir.Text = "Harga Grosir (Rp)";
            this.txtHargaGrosir.BackColor = System.Drawing.Color.White;
            this.txtHargaGrosir.BaseColor = System.Drawing.Color.White;
            this.txtHargaGrosir.BorderColorA = System.Drawing.Color.FromArgb(41, 128, 185);
            this.txtHargaGrosir.BorderColorB = System.Drawing.Color.Silver;
            this.txtHargaGrosir.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            this.txtHargaGrosir.Location = new System.Drawing.Point(186, 324);
            this.txtHargaGrosir.Width = 160;
            this.txtHargaGrosir.Height = 36;

            // Stok & Satuan
            this.lblStok.AutoSize = true;
            this.lblStok.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblStok.Location = new System.Drawing.Point(16, 366);
            this.lblStok.Text = "Jumlah Stok *";
            this.txtStok.BackColor = System.Drawing.Color.White;
            this.txtStok.BaseColor = System.Drawing.Color.White;
            this.txtStok.BorderColorA = System.Drawing.Color.FromArgb(41, 128, 185);
            this.txtStok.BorderColorB = System.Drawing.Color.Silver;
            this.txtStok.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            this.txtStok.Location = new System.Drawing.Point(16, 386);
            this.txtStok.Width = 160;
            this.txtStok.Height = 36;

            this.lblSatuan.AutoSize = true;
            this.lblSatuan.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblSatuan.Location = new System.Drawing.Point(186, 366);
            this.lblSatuan.Text = "Satuan";
            this.cmbSatuan.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.cmbSatuan.Location = new System.Drawing.Point(186, 388);
            this.cmbSatuan.Width = 160;
            this.cmbSatuan.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
            this.cmbSatuan.Items.AddRange(new object[] {
                "pcs", "kg", "gram", "liter", "box", "lusin", "pack", "botol", "bungkus", "porsi", "lembar"
            });

            // Tombol-Tombol
            this.btnSimpan.Location = new System.Drawing.Point(16, 436);
            this.btnSimpan.Width = 160;
            this.btnSimpan.Height = 38;
            this.btnSimpan.Text = "SIMPAN";
            this.btnSimpan.PrimaryColor = System.Drawing.Color.FromArgb(39, 174, 96);
            this.btnSimpan.Click += new System.EventHandler(this.btnSimpan_Click);

            this.btnEdit.Location = new System.Drawing.Point(186, 436);
            this.btnEdit.Width = 160;
            this.btnEdit.Height = 38;
            this.btnEdit.Text = "PERBARUI";
            this.btnEdit.PrimaryColor = System.Drawing.Color.FromArgb(243, 156, 18);
            this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);

            this.btnHapus.Location = new System.Drawing.Point(16, 480);
            this.btnHapus.Width = 160;
            this.btnHapus.Height = 38;
            this.btnHapus.Text = "HAPUS";
            this.btnHapus.PrimaryColor = System.Drawing.Color.FromArgb(231, 76, 60);
            this.btnHapus.Click += new System.EventHandler(this.btnHapus_Click);

            this.btnBersihkan.Location = new System.Drawing.Point(186, 480);
            this.btnBersihkan.Width = 160;
            this.btnBersihkan.Height = 38;
            this.btnBersihkan.Text = "BERSIHKAN";
            this.btnBersihkan.PrimaryColor = System.Drawing.Color.FromArgb(149, 165, 166);
            this.btnBersihkan.Click += new System.EventHandler(this.btnBersihkan_Click);

            // panelKanan
            this.panelKanan.BackColor = System.Drawing.Color.White;
            this.panelKanan.Controls.Add(this.dgvBarang);
            this.panelKanan.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelKanan.Padding = new System.Windows.Forms.Padding(16);

            // dgvBarang
            this.dgvBarang.BackgroundColor = System.Drawing.Color.White;
            this.dgvBarang.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvBarang.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvBarang.AllowUserToAddRows = false;
            this.dgvBarang.AllowUserToDeleteRows = false;
            this.dgvBarang.ReadOnly = true;
            this.dgvBarang.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvBarang.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.dgvBarang.RowTemplate.Height = 32;
            this.dgvBarang.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvBarang.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvBarang_CellClick);

            // FormBarang
            this.ClientSize = new System.Drawing.Size(1080, 640);
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
        private System.Windows.Forms.Label lblJenis;
        private System.Windows.Forms.ComboBox cmbJenis;
        private System.Windows.Forms.Label lblMerek;
        private System.Windows.Forms.ComboBox cmbMerek;
        private System.Windows.Forms.Label lblSupplier;
        private System.Windows.Forms.ComboBox cmbSupplier;
        private System.Windows.Forms.Label lblHargaBeli;
        private ReaLTaiizor.Controls.HopeTextBox txtHargaBeli;
        private System.Windows.Forms.Label lblHargaJual;
        private ReaLTaiizor.Controls.HopeTextBox txtHargaJual;
        private System.Windows.Forms.Label lblMinGrosir;
        private ReaLTaiizor.Controls.HopeTextBox txtMinGrosir;
        private System.Windows.Forms.Label lblHargaGrosir;
        private ReaLTaiizor.Controls.HopeTextBox txtHargaGrosir;
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