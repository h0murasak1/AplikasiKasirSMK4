namespace AplikasiKasirSMK4
{
    partial class Form1
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
            picLogo = new PictureBox();
            lblSubtitle = new Label();
            lblTitle = new Label();
            panelCard = new Panel();
            lblUsernameTitle = new Label();
            txtUsername = new TextBox();
            lblPasswordTitle = new Label();
            txtPassword = new TextBox();
            btnLogin = new ReaLTaiizor.Controls.HopeButton();
            lblFooter = new Label();
            panelHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picLogo).BeginInit();
            panelCard.SuspendLayout();
            SuspendLayout();
            // 
            // panelHeader
            // 
            panelHeader.BackColor = Color.FromArgb(15, 23, 42);
            panelHeader.Controls.Add(picLogo);
            panelHeader.Controls.Add(lblSubtitle);
            panelHeader.Controls.Add(lblTitle);
            panelHeader.Dock = DockStyle.Top;
            panelHeader.Location = new Point(0, 0);
            panelHeader.Name = "panelHeader";
            panelHeader.Padding = new Padding(24, 16, 24, 16);
            panelHeader.Size = new Size(420, 160);
            panelHeader.TabIndex = 1;
            // 
            // picLogo
            // 
            picLogo.Location = new Point(168, 12);
            picLogo.Name = "picLogo";
            picLogo.Size = new Size(84, 84);
            picLogo.SizeMode = PictureBoxSizeMode.Zoom;
            picLogo.TabIndex = 2;
            picLogo.TabStop = false;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = false;
            lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(0, 100);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(420, 30);
            lblTitle.TabIndex = 1;
            lblTitle.Text = "KASIR SMK NEGERI 4";
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblSubtitle
            // 
            lblSubtitle.AutoSize = false;
            lblSubtitle.Font = new Font("Segoe UI", 9F);
            lblSubtitle.ForeColor = Color.FromArgb(148, 163, 184);
            lblSubtitle.Location = new Point(0, 130);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(420, 22);
            lblSubtitle.TabIndex = 0;
            lblSubtitle.Text = "Sistem Point of Sales & Manajemen";
            lblSubtitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // panelCard
            // 
            panelCard.BackColor = Color.White;
            panelCard.BorderStyle = BorderStyle.FixedSingle;
            panelCard.Controls.Add(lblUsernameTitle);
            panelCard.Controls.Add(txtUsername);
            panelCard.Controls.Add(lblPasswordTitle);
            panelCard.Controls.Add(txtPassword);
            panelCard.Controls.Add(btnLogin);
            panelCard.Location = new Point(35, 178);
            panelCard.Name = "panelCard";
            panelCard.Padding = new Padding(24);
            panelCard.Size = new Size(350, 295);
            panelCard.TabIndex = 0;
            // 
            // lblUsernameTitle
            // 
            lblUsernameTitle.AutoSize = true;
            lblUsernameTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblUsernameTitle.ForeColor = Color.FromArgb(51, 65, 85);
            lblUsernameTitle.Location = new Point(24, 24);
            lblUsernameTitle.Name = "lblUsernameTitle";
            lblUsernameTitle.Size = new Size(91, 20);
            lblUsernameTitle.TabIndex = 0;
            lblUsernameTitle.Text = "USERNAME";
            // 
            // txtUsername
            // 
            txtUsername.BackColor = Color.FromArgb(248, 250, 252);
            txtUsername.BorderStyle = BorderStyle.FixedSingle;
            txtUsername.Font = new Font("Segoe UI", 13F);
            txtUsername.ForeColor = Color.FromArgb(15, 23, 42);
            txtUsername.Location = new Point(24, 50);
            txtUsername.Name = "txtUsername";
            txtUsername.Size = new Size(300, 36);
            txtUsername.TabIndex = 1;
            // 
            // lblPasswordTitle
            // 
            lblPasswordTitle.AutoSize = true;
            lblPasswordTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblPasswordTitle.ForeColor = Color.FromArgb(51, 65, 85);
            lblPasswordTitle.Location = new Point(24, 105);
            lblPasswordTitle.Name = "lblPasswordTitle";
            lblPasswordTitle.Size = new Size(93, 20);
            lblPasswordTitle.TabIndex = 2;
            lblPasswordTitle.Text = "PASSWORD";
            // 
            // txtPassword
            // 
            txtPassword.BackColor = Color.FromArgb(248, 250, 252);
            txtPassword.BorderStyle = BorderStyle.FixedSingle;
            txtPassword.Font = new Font("Segoe UI", 13F);
            txtPassword.ForeColor = Color.FromArgb(15, 23, 42);
            txtPassword.Location = new Point(24, 131);
            txtPassword.Name = "txtPassword";
            txtPassword.PasswordChar = '●';
            txtPassword.Size = new Size(300, 36);
            txtPassword.TabIndex = 3;
            // 
            // btnLogin
            // 
            btnLogin.BorderColor = Color.FromArgb(220, 223, 230);
            btnLogin.ButtonType = ReaLTaiizor.Util.HopeButtonType.Primary;
            btnLogin.Cursor = Cursors.Hand;
            btnLogin.DangerColor = Color.FromArgb(245, 108, 108);
            btnLogin.DefaultColor = Color.FromArgb(255, 255, 255);
            btnLogin.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnLogin.HoverTextColor = Color.White;
            btnLogin.InfoColor = Color.FromArgb(144, 147, 153);
            btnLogin.Location = new Point(24, 202);
            btnLogin.Name = "btnLogin";
            btnLogin.PrimaryColor = Color.FromArgb(37, 99, 235);
            btnLogin.Size = new Size(300, 52);
            btnLogin.SuccessColor = Color.FromArgb(46, 204, 113);
            btnLogin.TabIndex = 4;
            btnLogin.Text = "MASUK KE SISTEM";
            btnLogin.TextColor = Color.White;
            btnLogin.WarningColor = Color.FromArgb(230, 162, 60);
            btnLogin.Click += btnLogin_Click;
            // 
            // lblFooter
            // 
            lblFooter.Dock = DockStyle.Bottom;
            lblFooter.Font = new Font("Segoe UI", 8.5F);
            lblFooter.ForeColor = Color.FromArgb(100, 116, 139);
            lblFooter.Location = new Point(0, 492);
            lblFooter.Name = "lblFooter";
            lblFooter.Size = new Size(420, 30);
            lblFooter.TabIndex = 2;
            lblFooter.Text = "© SMK Negeri 4 Kabupaten Tangerang — v1.0.0";
            lblFooter.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // Form1
            // 
            BackColor = Color.FromArgb(241, 245, 249);
            ClientSize = new Size(420, 522);
            Controls.Add(lblFooter);
            Controls.Add(panelCard);
            Controls.Add(panelHeader);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Login — Kasir SMK Negeri 4";
            Load += Form1_Load;
            panelHeader.ResumeLayout(false);
            panelHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picLogo).EndInit();
            panelCard.ResumeLayout(false);
            panelCard.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel panelHeader;
        private PictureBox picLogo;
        private Label lblTitle;
        private Label lblSubtitle;
        private Panel panelCard;
        private Label lblUsernameTitle;
        private TextBox txtUsername;
        private Label lblPasswordTitle;
        private TextBox txtPassword;
        private ReaLTaiizor.Controls.HopeButton btnLogin;
        private Label lblFooter;
    }
}