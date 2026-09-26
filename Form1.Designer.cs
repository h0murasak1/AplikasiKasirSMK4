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
            lblSubtitle = new Label();
            lblTitle = new Label();
            panelCard = new Panel();
            lblUsernameTitle = new Label();
            txtUsername = new TextBox();
            lblPasswordTitle = new Label();
            txtPassword = new TextBox();
            btnLogin = new ReaLTaiizor.Controls.HopeButton();
            panelHeader.SuspendLayout();
            panelCard.SuspendLayout();
            SuspendLayout();
            // 
            // panelHeader
            // 
            panelHeader.BackColor = Color.FromArgb(41, 128, 185);
            panelHeader.Controls.Add(lblSubtitle);
            panelHeader.Controls.Add(lblTitle);
            panelHeader.Dock = DockStyle.Top;
            panelHeader.Location = new Point(0, 0);
            panelHeader.Name = "panelHeader";
            panelHeader.Size = new Size(400, 130);
            panelHeader.TabIndex = 1;
            // 
            // lblSubtitle
            // 
            lblSubtitle.AutoSize = true;
            lblSubtitle.Font = new Font("Segoe UI", 10F);
            lblSubtitle.ForeColor = Color.WhiteSmoke;
            lblSubtitle.Location = new Point(20, 80);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(245, 23);
            lblSubtitle.TabIndex = 0;
            lblSubtitle.Text = "APLIKASI KASIR SMK NEGERI 4";
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(12, 30);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(306, 54);
            lblTitle.TabIndex = 1;
            lblTitle.Text = "LOGIN SYSTEM";
            // 
            // panelCard
            // 
            panelCard.BackColor = Color.White;
            panelCard.Controls.Add(lblUsernameTitle);
            panelCard.Controls.Add(txtUsername);
            panelCard.Controls.Add(lblPasswordTitle);
            panelCard.Controls.Add(txtPassword);
            panelCard.Controls.Add(btnLogin);
            panelCard.Location = new Point(45, 170);
            panelCard.Name = "panelCard";
            panelCard.Padding = new Padding(20);
            panelCard.Size = new Size(310, 310);
            panelCard.TabIndex = 0;
            // 
            // lblUsernameTitle
            // 
            lblUsernameTitle.AutoSize = true;
            lblUsernameTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblUsernameTitle.ForeColor = Color.Gray;
            lblUsernameTitle.Location = new Point(20, 20);
            lblUsernameTitle.Name = "lblUsernameTitle";
            lblUsernameTitle.Size = new Size(102, 23);
            lblUsernameTitle.TabIndex = 0;
            lblUsernameTitle.Text = "USERNAME";
            // 
            // txtUsername
            // 
            txtUsername.BorderStyle = BorderStyle.FixedSingle;
            txtUsername.Font = new Font("Segoe UI", 14F);
            txtUsername.Location = new Point(24, 45);
            txtUsername.Name = "txtUsername";
            txtUsername.Size = new Size(260, 39);
            txtUsername.TabIndex = 1;
            // 
            // lblPasswordTitle
            // 
            lblPasswordTitle.AutoSize = true;
            lblPasswordTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblPasswordTitle.ForeColor = Color.Gray;
            lblPasswordTitle.Location = new Point(20, 100);
            lblPasswordTitle.Name = "lblPasswordTitle";
            lblPasswordTitle.Size = new Size(105, 23);
            lblPasswordTitle.TabIndex = 2;
            lblPasswordTitle.Text = "PASSWORD";
            // 
            // txtPassword
            // 
            txtPassword.BorderStyle = BorderStyle.FixedSingle;
            txtPassword.Font = new Font("Segoe UI", 14F);
            txtPassword.Location = new Point(24, 125);
            txtPassword.Name = "txtPassword";
            txtPassword.PasswordChar = '*';
            txtPassword.Size = new Size(260, 39);
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
            btnLogin.Location = new Point(24, 200);
            btnLogin.Name = "btnLogin";
            btnLogin.PrimaryColor = Color.FromArgb(41, 128, 185);
            btnLogin.Size = new Size(260, 50);
            btnLogin.SuccessColor = Color.FromArgb(46, 204, 113);
            btnLogin.TabIndex = 4;
            btnLogin.Text = "LOGIN";
            btnLogin.TextColor = Color.White;
            btnLogin.WarningColor = Color.FromArgb(230, 162, 60);
            btnLogin.Click += btnLogin_Click;
            // 
            // Form1
            // 
            BackColor = Color.WhiteSmoke;
            ClientSize = new Size(400, 520);
            Controls.Add(panelCard);
            Controls.Add(panelHeader);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Login - Kasir SMK N 4";
            Load += Form1_Load;
            panelHeader.ResumeLayout(false);
            panelHeader.PerformLayout();
            panelCard.ResumeLayout(false);
            panelCard.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.Panel panelCard;
        private System.Windows.Forms.Label lblUsernameTitle;
        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.Label lblPasswordTitle;
        private System.Windows.Forms.TextBox txtPassword;
        private ReaLTaiizor.Controls.HopeButton btnLogin;
    }
}