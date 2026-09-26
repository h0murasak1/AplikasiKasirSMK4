namespace AplikasiKasirSMK4
{
    partial class FormKasir
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
            panelHeader = new Panel();
            lblJudul = new Label();
            panelKanan = new Panel();
            lblTotalTitle = new Label();
            lblTotal = new Label();
            lblBayarTitle = new Label();
            txtBayar = new ReaLTaiizor.Controls.HopeTextBox();
            lblKembalianTitle = new Label();
            lblKembalian = new Label();
            btnBayar = new ReaLTaiizor.Controls.HopeButton();
            panelTengah = new Panel();
            dgvKeranjang = new DataGridView();
            Kode = new DataGridViewTextBoxColumn();
            Nama = new DataGridViewTextBoxColumn();
            Harga = new DataGridViewTextBoxColumn();
            Qty = new DataGridViewTextBoxColumn();
            Subtotal = new DataGridViewTextBoxColumn();
            txtBarcode = new ReaLTaiizor.Controls.HopeTextBox();
            panelHeader.SuspendLayout();
            panelKanan.SuspendLayout();
            panelTengah.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvKeranjang).BeginInit();
            SuspendLayout();
            // 
            // panelHeader
            // 
            panelHeader.BackColor = Color.FromArgb(41, 128, 185);
            panelHeader.Controls.Add(lblJudul);
            panelHeader.Dock = DockStyle.Top;
            panelHeader.Location = new Point(0, 0);
            panelHeader.Name = "panelHeader";
            panelHeader.Size = new Size(1000, 70);
            panelHeader.TabIndex = 2;
            // 
            // lblJudul
            // 
            lblJudul.AutoSize = true;
            lblJudul.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblJudul.ForeColor = Color.White;
            lblJudul.Location = new Point(20, 18);
            lblJudul.Name = "lblJudul";
            lblJudul.Size = new Size(457, 41);
            lblJudul.TabIndex = 0;
            lblJudul.Text = "APLIKASI KASIR SMK NEGERI 4";
            // 
            // panelKanan
            // 
            panelKanan.BackColor = Color.WhiteSmoke;
            panelKanan.Controls.Add(lblTotalTitle);
            panelKanan.Controls.Add(lblTotal);
            panelKanan.Controls.Add(lblBayarTitle);
            panelKanan.Controls.Add(txtBayar);
            panelKanan.Controls.Add(lblKembalianTitle);
            panelKanan.Controls.Add(lblKembalian);
            panelKanan.Controls.Add(btnBayar);
            panelKanan.Dock = DockStyle.Right;
            panelKanan.Location = new Point(650, 70);
            panelKanan.Name = "panelKanan";
            panelKanan.Padding = new Padding(20);
            panelKanan.Size = new Size(350, 530);
            panelKanan.TabIndex = 1;
            // 
            // lblTotalTitle
            // 
            lblTotalTitle.AutoSize = true;
            lblTotalTitle.Font = new Font("Segoe UI", 12F);
            lblTotalTitle.Location = new Point(20, 20);
            lblTotalTitle.Name = "lblTotalTitle";
            lblTotalTitle.Size = new Size(150, 28);
            lblTotalTitle.TabIndex = 0;
            lblTotalTitle.Text = "TOTAL BELANJA";
            // 
            // lblTotal
            // 
            lblTotal.BackColor = Color.Black;
            lblTotal.Font = new Font("Consolas", 36F, FontStyle.Bold);
            lblTotal.ForeColor = Color.Lime;
            lblTotal.Location = new Point(20, 50);
            lblTotal.Name = "lblTotal";
            lblTotal.Size = new Size(310, 80);
            lblTotal.TabIndex = 1;
            lblTotal.Text = "0";
            lblTotal.TextAlign = ContentAlignment.MiddleRight;
            // 
            // lblBayarTitle
            // 
            lblBayarTitle.AutoSize = true;
            lblBayarTitle.Font = new Font("Segoe UI", 12F);
            lblBayarTitle.Location = new Point(20, 150);
            lblBayarTitle.Name = "lblBayarTitle";
            lblBayarTitle.Size = new Size(153, 28);
            lblBayarTitle.TabIndex = 2;
            lblBayarTitle.Text = "Uang Bayar (Rp)";
            // 
            // txtBayar
            // 
            txtBayar.BackColor = Color.White;
            txtBayar.BaseColor = Color.White;
            txtBayar.BorderColorA = Color.FromArgb(41, 128, 185);
            txtBayar.BorderColorB = Color.Silver;
            txtBayar.Font = new Font("Segoe UI", 14F);
            txtBayar.ForeColor = Color.FromArgb(48, 49, 51);
            txtBayar.Hint = "";
            txtBayar.Location = new Point(20, 180);
            txtBayar.MaxLength = 32767;
            txtBayar.Multiline = false;
            txtBayar.Name = "txtBayar";
            txtBayar.PasswordChar = '\0';
            txtBayar.ScrollBars = ScrollBars.None;
            txtBayar.SelectedText = "";
            txtBayar.SelectionLength = 0;
            txtBayar.SelectionStart = 0;
            txtBayar.Size = new Size(310, 48);
            txtBayar.TabIndex = 3;
            txtBayar.TabStop = false;
            txtBayar.UseSystemPasswordChar = false;
            txtBayar.TextChanged += txtBayar_TextChanged;
            // 
            // lblKembalianTitle
            // 
            lblKembalianTitle.AutoSize = true;
            lblKembalianTitle.Font = new Font("Segoe UI", 12F);
            lblKembalianTitle.Location = new Point(20, 240);
            lblKembalianTitle.Name = "lblKembalianTitle";
            lblKembalianTitle.Size = new Size(145, 28);
            lblKembalianTitle.TabIndex = 4;
            lblKembalianTitle.Text = "Kembalian (Rp)";
            // 
            // lblKembalian
            // 
            lblKembalian.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
            lblKembalian.ForeColor = Color.OrangeRed;
            lblKembalian.Location = new Point(20, 270);
            lblKembalian.Name = "lblKembalian";
            lblKembalian.Size = new Size(310, 55);
            lblKembalian.TabIndex = 5;
            lblKembalian.Text = "0";
            lblKembalian.TextAlign = ContentAlignment.MiddleRight;
            // 
            // btnBayar
            // 
            btnBayar.BorderColor = Color.FromArgb(220, 223, 230);
            btnBayar.ButtonType = ReaLTaiizor.Util.HopeButtonType.Primary;
            btnBayar.DangerColor = Color.FromArgb(245, 108, 108);
            btnBayar.DefaultColor = Color.FromArgb(255, 255, 255);
            btnBayar.Dock = DockStyle.Bottom;
            btnBayar.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            btnBayar.HoverTextColor = Color.White;
            btnBayar.InfoColor = Color.FromArgb(144, 147, 153);
            btnBayar.Location = new Point(20, 450);
            btnBayar.Name = "btnBayar";
            btnBayar.PrimaryColor = Color.FromArgb(39, 174, 96);
            btnBayar.Size = new Size(310, 60);
            btnBayar.SuccessColor = Color.FromArgb(46, 204, 113);
            btnBayar.TabIndex = 6;
            btnBayar.Text = "BAYAR & SIMPAN";
            btnBayar.TextColor = Color.White;
            btnBayar.WarningColor = Color.FromArgb(230, 162, 60);
            btnBayar.Click += btnBayar_Click;
            // 
            // panelTengah
            // 
            panelTengah.BackColor = Color.White;
            panelTengah.Controls.Add(dgvKeranjang);
            panelTengah.Controls.Add(txtBarcode);
            panelTengah.Dock = DockStyle.Fill;
            panelTengah.Location = new Point(0, 70);
            panelTengah.Name = "panelTengah";
            panelTengah.Padding = new Padding(20);
            panelTengah.Size = new Size(650, 530);
            panelTengah.TabIndex = 0;
            // 
            // dgvKeranjang
            // 
            dgvKeranjang.AllowUserToAddRows = false;
            dgvKeranjang.AllowUserToDeleteRows = false;
            dgvKeranjang.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvKeranjang.BackgroundColor = Color.White;
            dgvKeranjang.BorderStyle = BorderStyle.None;
            dgvKeranjang.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvKeranjang.Columns.AddRange(new DataGridViewColumn[] { Kode, Nama, Harga, Qty, Subtotal });
            dgvKeranjang.Dock = DockStyle.Fill;
            dgvKeranjang.Font = new Font("Segoe UI", 12F);
            dgvKeranjang.Location = new Point(20, 68);
            dgvKeranjang.Name = "dgvKeranjang";
            dgvKeranjang.ReadOnly = true;
            dgvKeranjang.RowHeadersWidth = 51;
            dgvKeranjang.RowTemplate.Height = 35;
            dgvKeranjang.Size = new Size(610, 442);
            dgvKeranjang.TabIndex = 0;
            // 
            // Kode
            // 
            Kode.HeaderText = "Kode";
            Kode.MinimumWidth = 6;
            Kode.Name = "Kode";
            Kode.ReadOnly = true;
            // 
            // Nama
            // 
            Nama.HeaderText = "Nama Barang";
            Nama.MinimumWidth = 6;
            Nama.Name = "Nama";
            Nama.ReadOnly = true;
            // 
            // Harga
            // 
            Harga.HeaderText = "Harga";
            Harga.MinimumWidth = 6;
            Harga.Name = "Harga";
            Harga.ReadOnly = true;
            // 
            // Qty
            // 
            Qty.HeaderText = "Qty";
            Qty.MinimumWidth = 6;
            Qty.Name = "Qty";
            Qty.ReadOnly = true;
            // 
            // Subtotal
            // 
            Subtotal.HeaderText = "Subtotal";
            Subtotal.MinimumWidth = 6;
            Subtotal.Name = "Subtotal";
            Subtotal.ReadOnly = true;
            // 
            // txtBarcode
            // 
            txtBarcode.BackColor = Color.White;
            txtBarcode.BaseColor = Color.White;
            txtBarcode.BorderColorA = Color.FromArgb(41, 128, 185);
            txtBarcode.BorderColorB = Color.Silver;
            txtBarcode.Dock = DockStyle.Top;
            txtBarcode.Font = new Font("Segoe UI", 14F);
            txtBarcode.ForeColor = Color.FromArgb(48, 49, 51);
            txtBarcode.Hint = "";
            txtBarcode.Location = new Point(20, 20);
            txtBarcode.MaxLength = 32767;
            txtBarcode.Multiline = false;
            txtBarcode.Name = "txtBarcode";
            txtBarcode.PasswordChar = '\0';
            txtBarcode.ScrollBars = ScrollBars.None;
            txtBarcode.SelectedText = "";
            txtBarcode.SelectionLength = 0;
            txtBarcode.SelectionStart = 0;
            txtBarcode.Size = new Size(610, 48);
            txtBarcode.TabIndex = 1;
            txtBarcode.TabStop = false;
            txtBarcode.UseSystemPasswordChar = false;
            txtBarcode.KeyDown += txtBarcode_KeyDown;
            // 
            // FormKasir
            // 
            ClientSize = new Size(1000, 600);
            Controls.Add(panelTengah);
            Controls.Add(panelKanan);
            Controls.Add(panelHeader);
            Name = "FormKasir";
            Text = "Kasir - SMK Negeri 4";
            WindowState = FormWindowState.Maximized;
            panelHeader.ResumeLayout(false);
            panelHeader.PerformLayout();
            panelKanan.ResumeLayout(false);
            panelKanan.PerformLayout();
            panelTengah.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvKeranjang).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.Label lblJudul;
        private System.Windows.Forms.Panel panelKanan;
        private System.Windows.Forms.Label lblTotalTitle;
        private System.Windows.Forms.Label lblTotal;
        private System.Windows.Forms.Label lblBayarTitle;
        private ReaLTaiizor.Controls.HopeTextBox txtBayar;
        private System.Windows.Forms.Label lblKembalianTitle;
        private System.Windows.Forms.Label lblKembalian;
        private ReaLTaiizor.Controls.HopeButton btnBayar;
        private System.Windows.Forms.Panel panelTengah;
        private ReaLTaiizor.Controls.HopeTextBox txtBarcode;
        private System.Windows.Forms.DataGridView dgvKeranjang;
        private System.Windows.Forms.DataGridViewTextBoxColumn Kode;
        private System.Windows.Forms.DataGridViewTextBoxColumn Nama;
        private System.Windows.Forms.DataGridViewTextBoxColumn Harga;
        private System.Windows.Forms.DataGridViewTextBoxColumn Qty;
        private System.Windows.Forms.DataGridViewTextBoxColumn Subtotal;
    }
}