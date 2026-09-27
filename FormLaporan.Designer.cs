namespace AplikasiKasirSMK4
{
    partial class FormLaporan
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
            this.lblSubjudul = new System.Windows.Forms.Label();
            this.panelFilter = new System.Windows.Forms.Panel();
            this.lblDari = new System.Windows.Forms.Label();
            this.dtpMulai = new System.Windows.Forms.DateTimePicker();
            this.lblSampai = new System.Windows.Forms.Label();
            this.dtpSelesai = new System.Windows.Forms.DateTimePicker();
            this.btnHariIni = new System.Windows.Forms.Button();
            this.btnBulanIni = new System.Windows.Forms.Button();
            this.lblKasir = new System.Windows.Forms.Label();
            this.cmbKasir = new System.Windows.Forms.ComboBox();
            this.btnFilter = new ReaLTaiizor.Controls.HopeButton();
            this.btnEkspor = new ReaLTaiizor.Controls.HopeButton();
            this.panelSummary = new System.Windows.Forms.Panel();
            this.panelKpiOmzet = new System.Windows.Forms.Panel();
            this.lblJudulOmzet = new System.Windows.Forms.Label();
            this.lblValOmzet = new System.Windows.Forms.Label();
            this.panelKpiTransaksi = new System.Windows.Forms.Panel();
            this.lblJudulTransaksi = new System.Windows.Forms.Label();
            this.lblValTransaksi = new System.Windows.Forms.Label();
            this.panelKpiQty = new System.Windows.Forms.Panel();
            this.lblJudulQty = new System.Windows.Forms.Label();
            this.lblValQty = new System.Windows.Forms.Label();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabNota = new System.Windows.Forms.TabPage();
            this.splitNota = new System.Windows.Forms.SplitContainer();
            this.dgvNota = new System.Windows.Forms.DataGridView();
            this.panelDetailHeader = new System.Windows.Forms.Panel();
            this.lblDetailNota = new System.Windows.Forms.Label();
            this.dgvDetailNota = new System.Windows.Forms.DataGridView();
            this.tabDetail = new System.Windows.Forms.TabPage();
            this.dgvDetail = new System.Windows.Forms.DataGridView();
            this.tabProduk = new System.Windows.Forms.TabPage();
            this.dgvProduk = new System.Windows.Forms.DataGridView();
            this.panelHeader.SuspendLayout();
            this.panelFilter.SuspendLayout();
            this.panelSummary.SuspendLayout();
            this.panelKpiOmzet.SuspendLayout();
            this.panelKpiTransaksi.SuspendLayout();
            this.panelKpiQty.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.tabNota.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitNota)).BeginInit();
            this.splitNota.Panel1.SuspendLayout();
            this.splitNota.Panel2.SuspendLayout();
            this.splitNota.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvNota)).BeginInit();
            this.panelDetailHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDetailNota)).BeginInit();
            this.tabDetail.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDetail)).BeginInit();
            this.tabProduk.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProduk)).BeginInit();
            this.SuspendLayout();

            // panelHeader
            this.panelHeader.BackColor = System.Drawing.Color.FromArgb(41, 128, 185);
            this.panelHeader.Controls.Add(this.lblSubjudul);
            this.panelHeader.Controls.Add(this.lblJudul);
            this.panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHeader.Height = 70;

            // lblJudul
            this.lblJudul.AutoSize = true;
            this.lblJudul.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblJudul.ForeColor = System.Drawing.Color.White;
            this.lblJudul.Location = new System.Drawing.Point(20, 10);
            this.lblJudul.Text = "LAPORAN PENJUALAN & TRANSAKSI";

            // lblSubjudul
            this.lblSubjudul.AutoSize = true;
            this.lblSubjudul.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblSubjudul.ForeColor = System.Drawing.Color.FromArgb(236, 240, 241);
            this.lblSubjudul.Location = new System.Drawing.Point(22, 40);
            this.lblSubjudul.Text = "SMK Negeri 4 - Filter periode, rekap nota, rincian barang & ekspor CSV/Excel";

            // panelFilter
            this.panelFilter.BackColor = System.Drawing.Color.WhiteSmoke;
            this.panelFilter.Controls.Add(this.lblDari);
            this.panelFilter.Controls.Add(this.dtpMulai);
            this.panelFilter.Controls.Add(this.lblSampai);
            this.panelFilter.Controls.Add(this.dtpSelesai);
            this.panelFilter.Controls.Add(this.btnHariIni);
            this.panelFilter.Controls.Add(this.btnBulanIni);
            this.panelFilter.Controls.Add(this.lblKasir);
            this.panelFilter.Controls.Add(this.cmbKasir);
            this.panelFilter.Controls.Add(this.btnFilter);
            this.panelFilter.Controls.Add(this.btnEkspor);
            this.panelFilter.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelFilter.Height = 65;
            this.panelFilter.Padding = new System.Windows.Forms.Padding(15, 10, 15, 10);

            // lblDari & dtpMulai
            this.lblDari.AutoSize = true;
            this.lblDari.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblDari.Location = new System.Drawing.Point(15, 22);
            this.lblDari.Text = "Dari:";

            this.dtpMulai.CustomFormat = "dd/MM/yyyy";
            this.dtpMulai.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.dtpMulai.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpMulai.Location = new System.Drawing.Point(55, 18);
            this.dtpMulai.Size = new System.Drawing.Size(120, 27);

            // lblSampai & dtpSelesai
            this.lblSampai.AutoSize = true;
            this.lblSampai.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblSampai.Location = new System.Drawing.Point(185, 22);
            this.lblSampai.Text = "s/d:";

            this.dtpSelesai.CustomFormat = "dd/MM/yyyy";
            this.dtpSelesai.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.dtpSelesai.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpSelesai.Location = new System.Drawing.Point(220, 18);
            this.dtpSelesai.Size = new System.Drawing.Size(120, 27);

            // btnHariIni & btnBulanIni
            this.btnHariIni.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.btnHariIni.Location = new System.Drawing.Point(350, 18);
            this.btnHariIni.Size = new System.Drawing.Size(70, 27);
            this.btnHariIni.Text = "Hari Ini";
            this.btnHariIni.UseVisualStyleBackColor = true;
            this.btnHariIni.Click += new System.EventHandler(this.btnHariIni_Click);

            this.btnBulanIni.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.btnBulanIni.Location = new System.Drawing.Point(425, 18);
            this.btnBulanIni.Size = new System.Drawing.Size(75, 27);
            this.btnBulanIni.Text = "Bulan Ini";
            this.btnBulanIni.UseVisualStyleBackColor = true;
            this.btnBulanIni.Click += new System.EventHandler(this.btnBulanIni_Click);

            // lblKasir & cmbKasir
            this.lblKasir.AutoSize = true;
            this.lblKasir.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblKasir.Location = new System.Drawing.Point(515, 22);
            this.lblKasir.Text = "Kasir:";

            this.cmbKasir.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbKasir.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.cmbKasir.Location = new System.Drawing.Point(560, 19);
            this.cmbKasir.Size = new System.Drawing.Size(150, 25);

            // btnFilter
            this.btnFilter.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnFilter.Location = new System.Drawing.Point(725, 15);
            this.btnFilter.PrimaryColor = System.Drawing.Color.FromArgb(41, 128, 185);
            this.btnFilter.Size = new System.Drawing.Size(110, 34);
            this.btnFilter.Text = "TAMPILKAN";
            this.btnFilter.Click += new System.EventHandler(this.btnFilter_Click);

            // btnEkspor
            this.btnEkspor.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnEkspor.Location = new System.Drawing.Point(845, 15);
            this.btnEkspor.PrimaryColor = System.Drawing.Color.FromArgb(39, 174, 96);
            this.btnEkspor.Size = new System.Drawing.Size(140, 34);
            this.btnEkspor.Text = "EKSPOR CSV";
            this.btnEkspor.Click += new System.EventHandler(this.btnEkspor_Click);

            // panelSummary
            this.panelSummary.BackColor = System.Drawing.Color.White;
            this.panelSummary.Controls.Add(this.panelKpiOmzet);
            this.panelSummary.Controls.Add(this.panelKpiTransaksi);
            this.panelSummary.Controls.Add(this.panelKpiQty);
            this.panelSummary.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelSummary.Height = 70;
            this.panelSummary.Padding = new System.Windows.Forms.Padding(15, 5, 15, 5);

            // panelKpiOmzet
            this.panelKpiOmzet.BackColor = System.Drawing.Color.FromArgb(235, 247, 238);
            this.panelKpiOmzet.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelKpiOmzet.Controls.Add(this.lblJudulOmzet);
            this.panelKpiOmzet.Controls.Add(this.lblValOmzet);
            this.panelKpiOmzet.Location = new System.Drawing.Point(20, 8);
            this.panelKpiOmzet.Size = new System.Drawing.Size(300, 52);

            this.lblJudulOmzet.AutoSize = true;
            this.lblJudulOmzet.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblJudulOmzet.ForeColor = System.Drawing.Color.FromArgb(39, 174, 96);
            this.lblJudulOmzet.Location = new System.Drawing.Point(10, 5);
            this.lblJudulOmzet.Text = "TOTAL OMZET PENJUALAN";

            this.lblValOmzet.AutoSize = true;
            this.lblValOmzet.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.lblValOmzet.ForeColor = System.Drawing.Color.FromArgb(39, 174, 96);
            this.lblValOmzet.Location = new System.Drawing.Point(10, 22);
            this.lblValOmzet.Text = "Rp 0";

            // panelKpiTransaksi
            this.panelKpiTransaksi.BackColor = System.Drawing.Color.FromArgb(235, 245, 255);
            this.panelKpiTransaksi.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelKpiTransaksi.Controls.Add(this.lblJudulTransaksi);
            this.panelKpiTransaksi.Controls.Add(this.lblValTransaksi);
            this.panelKpiTransaksi.Location = new System.Drawing.Point(340, 8);
            this.panelKpiTransaksi.Size = new System.Drawing.Size(250, 52);

            this.lblJudulTransaksi.AutoSize = true;
            this.lblJudulTransaksi.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblJudulTransaksi.ForeColor = System.Drawing.Color.FromArgb(41, 128, 185);
            this.lblJudulTransaksi.Location = new System.Drawing.Point(10, 5);
            this.lblJudulTransaksi.Text = "JUMLAH TRANSAKSI / NOTA";

            this.lblValTransaksi.AutoSize = true;
            this.lblValTransaksi.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.lblValTransaksi.ForeColor = System.Drawing.Color.FromArgb(41, 128, 185);
            this.lblValTransaksi.Location = new System.Drawing.Point(10, 22);
            this.lblValTransaksi.Text = "0 Nota";

            // panelKpiQty
            this.panelKpiQty.BackColor = System.Drawing.Color.FromArgb(254, 249, 231);
            this.panelKpiQty.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelKpiQty.Controls.Add(this.lblJudulQty);
            this.panelKpiQty.Controls.Add(this.lblValQty);
            this.panelKpiQty.Location = new System.Drawing.Point(610, 8);
            this.panelKpiQty.Size = new System.Drawing.Size(250, 52);

            this.lblJudulQty.AutoSize = true;
            this.lblJudulQty.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            this.lblJudulQty.ForeColor = System.Drawing.Color.FromArgb(214, 137, 16);
            this.lblJudulQty.Location = new System.Drawing.Point(10, 5);
            this.lblJudulQty.Text = "TOTAL BARANG TERJUAL";

            this.lblValQty.AutoSize = true;
            this.lblValQty.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.lblValQty.ForeColor = System.Drawing.Color.FromArgb(214, 137, 16);
            this.lblValQty.Location = new System.Drawing.Point(10, 22);
            this.lblValQty.Text = "0 Item";

            // tabControl
            this.tabControl.Controls.Add(this.tabNota);
            this.tabControl.Controls.Add(this.tabDetail);
            this.tabControl.Controls.Add(this.tabProduk);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.tabControl.Location = new System.Drawing.Point(0, 205);
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(1100, 495);

            // tabNota
            this.tabNota.Controls.Add(this.splitNota);
            this.tabNota.Location = new System.Drawing.Point(4, 26);
            this.tabNota.Padding = new System.Windows.Forms.Padding(10);
            this.tabNota.Text = "Rekap Transaksi (Per Nota)";
            this.tabNota.UseVisualStyleBackColor = true;

            // splitNota
            this.splitNota.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitNota.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.splitNota.SplitterDistance = 240;

            // dgvNota
            this.dgvNota.AllowUserToAddRows = false;
            this.dgvNota.AllowUserToDeleteRows = false;
            this.dgvNota.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvNota.BackgroundColor = System.Drawing.Color.White;
            this.dgvNota.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dgvNota.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvNota.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.dgvNota.ReadOnly = true;
            this.dgvNota.RowTemplate.Height = 30;
            this.dgvNota.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvNota.SelectionChanged += new System.EventHandler(this.dgvNota_SelectionChanged);
            this.dgvNota.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dgvNota_CellFormatting);

            // panelDetailHeader
            this.panelDetailHeader.BackColor = System.Drawing.Color.FromArgb(240, 243, 244);
            this.panelDetailHeader.Controls.Add(this.lblDetailNota);
            this.panelDetailHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelDetailHeader.Height = 28;

            this.lblDetailNota.AutoSize = true;
            this.lblDetailNota.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblDetailNota.ForeColor = System.Drawing.Color.FromArgb(52, 73, 94);
            this.lblDetailNota.Location = new System.Drawing.Point(8, 6);
            this.lblDetailNota.Text = "Rincian Item Pada Nota Terpilih:";

            // dgvDetailNota
            this.dgvDetailNota.AllowUserToAddRows = false;
            this.dgvDetailNota.AllowUserToDeleteRows = false;
            this.dgvDetailNota.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvDetailNota.BackgroundColor = System.Drawing.Color.White;
            this.dgvDetailNota.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dgvDetailNota.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvDetailNota.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.dgvDetailNota.ReadOnly = true;
            this.dgvDetailNota.RowTemplate.Height = 28;
            this.dgvDetailNota.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvDetailNota.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dgvDetailNota_CellFormatting);

            // splitNota panel assembling
            this.splitNota.Panel1.Controls.Add(this.dgvNota);
            this.splitNota.Panel2.Controls.Add(this.dgvDetailNota);
            this.splitNota.Panel2.Controls.Add(this.panelDetailHeader);

            // tabDetail
            this.tabDetail.Controls.Add(this.dgvDetail);
            this.tabDetail.Location = new System.Drawing.Point(4, 26);
            this.tabDetail.Padding = new System.Windows.Forms.Padding(10);
            this.tabDetail.Text = "Rincian Penjualan (Semua Item)";
            this.tabDetail.UseVisualStyleBackColor = true;

            // dgvDetail
            this.dgvDetail.AllowUserToAddRows = false;
            this.dgvDetail.AllowUserToDeleteRows = false;
            this.dgvDetail.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvDetail.BackgroundColor = System.Drawing.Color.White;
            this.dgvDetail.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dgvDetail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvDetail.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.dgvDetail.ReadOnly = true;
            this.dgvDetail.RowTemplate.Height = 30;
            this.dgvDetail.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvDetail.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dgvDetail_CellFormatting);

            // tabProduk
            this.tabProduk.Controls.Add(this.dgvProduk);
            this.tabProduk.Location = new System.Drawing.Point(4, 26);
            this.tabProduk.Padding = new System.Windows.Forms.Padding(10);
            this.tabProduk.Text = "Produk Terlaris (Ranking)";
            this.tabProduk.UseVisualStyleBackColor = true;

            // dgvProduk
            this.dgvProduk.AllowUserToAddRows = false;
            this.dgvProduk.AllowUserToDeleteRows = false;
            this.dgvProduk.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvProduk.BackgroundColor = System.Drawing.Color.White;
            this.dgvProduk.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dgvProduk.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvProduk.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.dgvProduk.ReadOnly = true;
            this.dgvProduk.RowTemplate.Height = 30;
            this.dgvProduk.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvProduk.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dgvProduk_CellFormatting);

            // FormLaporan
            this.ClientSize = new System.Drawing.Size(1100, 700);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.panelSummary);
            this.Controls.Add(this.panelFilter);
            this.Controls.Add(this.panelHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(950, 600);
            this.Name = "FormLaporan";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Laporan Penjualan - SMK Negeri 4";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.FormLaporan_Load);

            this.panelHeader.ResumeLayout(false);
            this.panelHeader.PerformLayout();
            this.panelFilter.ResumeLayout(false);
            this.panelFilter.PerformLayout();
            this.panelSummary.ResumeLayout(false);
            this.panelKpiOmzet.ResumeLayout(false);
            this.panelKpiOmzet.PerformLayout();
            this.panelKpiTransaksi.ResumeLayout(false);
            this.panelKpiTransaksi.PerformLayout();
            this.panelKpiQty.ResumeLayout(false);
            this.panelKpiQty.PerformLayout();
            this.tabControl.ResumeLayout(false);
            this.tabNota.ResumeLayout(false);
            this.splitNota.Panel1.ResumeLayout(false);
            this.splitNota.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitNota)).EndInit();
            this.splitNota.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvNota)).EndInit();
            this.panelDetailHeader.ResumeLayout(false);
            this.panelDetailHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDetailNota)).EndInit();
            this.tabDetail.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvDetail)).EndInit();
            this.tabProduk.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvProduk)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.Label lblJudul;
        private System.Windows.Forms.Label lblSubjudul;
        private System.Windows.Forms.Panel panelFilter;
        private System.Windows.Forms.Label lblDari;
        private System.Windows.Forms.DateTimePicker dtpMulai;
        private System.Windows.Forms.Label lblSampai;
        private System.Windows.Forms.DateTimePicker dtpSelesai;
        private System.Windows.Forms.Button btnHariIni;
        private System.Windows.Forms.Button btnBulanIni;
        private System.Windows.Forms.Label lblKasir;
        private System.Windows.Forms.ComboBox cmbKasir;
        private ReaLTaiizor.Controls.HopeButton btnFilter;
        private ReaLTaiizor.Controls.HopeButton btnEkspor;
        private System.Windows.Forms.Panel panelSummary;
        private System.Windows.Forms.Panel panelKpiOmzet;
        private System.Windows.Forms.Label lblJudulOmzet;
        private System.Windows.Forms.Label lblValOmzet;
        private System.Windows.Forms.Panel panelKpiTransaksi;
        private System.Windows.Forms.Label lblJudulTransaksi;
        private System.Windows.Forms.Label lblValTransaksi;
        private System.Windows.Forms.Panel panelKpiQty;
        private System.Windows.Forms.Label lblJudulQty;
        private System.Windows.Forms.Label lblValQty;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabNota;
        private System.Windows.Forms.SplitContainer splitNota;
        private System.Windows.Forms.DataGridView dgvNota;
        private System.Windows.Forms.Panel panelDetailHeader;
        private System.Windows.Forms.Label lblDetailNota;
        private System.Windows.Forms.DataGridView dgvDetailNota;
        private System.Windows.Forms.TabPage tabDetail;
        private System.Windows.Forms.DataGridView dgvDetail;
        private System.Windows.Forms.TabPage tabProduk;
        private System.Windows.Forms.DataGridView dgvProduk;
    }
}
