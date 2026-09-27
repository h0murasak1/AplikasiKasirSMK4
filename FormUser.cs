using Microsoft.Data.Sqlite;

namespace AplikasiKasirSMK4
{
    /// <summary>
    /// Form untuk mengelola akun pengguna aplikasi.
    /// </summary>
    /// <remarks>
    /// Admin bisa menambah user baru, mengubah nama dan role, mereset
    /// password, dan menonaktifkan akun. Akun yang dinonaktifkan tidak
    /// bisa login tetapi datanya tetap ada di database karena bisa jadi
    /// ia sudah terlanjur membuat transaksi.
    /// <para>
    /// Admin tidak bisa menghapus akunnya sendiri. Nonaktifkan pun
    /// tidak diizinkan supaya tidak pernah ada kondisi "tidak ada admin
    /// yang bisa login".
    /// </para>
    /// </remarks>
    public class FormUser : Form
    {
        // ─── Kontrol ────────────────────────────────────────────────
        private readonly DataGridView dgvUser = new();
        private readonly Button btnTambah   = new();
        private readonly Button btnEdit     = new();
        private readonly Button btnPassword = new();
        private readonly Button btnAktif    = new();
        private readonly Button btnRefresh  = new();
        private readonly CheckBox chkTampilNonaktif = new();

        private readonly Koneksi _koneksi = new();

        // ─── Konstanta kolom dgv ────────────────────────────────────
        private const int KolId       = 0;
        private const int KolNama     = 1;
        private const int KolUsername = 2;
        private const int KolRole     = 3;
        private const int KolAktif    = 4;
        private const int KolDibuat   = 5;

        public FormUser()
        {
            Text            = "Manajemen User";
            StartPosition   = FormStartPosition.CenterParent;
            MinimumSize     = new Size(700, 480);
            ClientSize      = new Size(750, 520);
            Font            = new Font("Segoe UI", 9F);

            BangunTampilan();

            Load    += FormUser_Load;
            Resize  += (_, _) => SesuaikanLayout();
        }

        // ─── BANGUN TAMPILAN ─────────────────────────────────────────

        private void BangunTampilan()
        {
            // Panel tombol atas
            var panelAtas = new Panel
            {
                Dock   = DockStyle.Top,
                Height = 48,
                Padding = new Padding(8, 8, 8, 4)
            };

            KonfigurasiTombol(btnTambah,   "＋ Tambah User",   Color.FromArgb(39, 174, 96));
            KonfigurasiTombol(btnEdit,     "✎ Edit",           Color.FromArgb(52, 152, 219));
            KonfigurasiTombol(btnPassword, "🔑 Reset Password", Color.FromArgb(243, 156, 18));
            KonfigurasiTombol(btnAktif,    "⊘ Nonaktifkan",    Color.FromArgb(231, 76, 60));
            KonfigurasiTombol(btnRefresh,  "↻ Muat Ulang",     Color.FromArgb(100, 100, 100));

            chkTampilNonaktif.Text      = "Tampilkan nonaktif";
            chkTampilNonaktif.AutoSize  = true;
            chkTampilNonaktif.Anchor    = AnchorStyles.Left | AnchorStyles.Top;

            panelAtas.Controls.AddRange(new Control[]
                { btnTambah, btnEdit, btnPassword, btnAktif, btnRefresh, chkTampilNonaktif });

            // DataGridView
            dgvUser.Dock                  = DockStyle.Fill;
            dgvUser.ReadOnly              = true;
            dgvUser.SelectionMode         = DataGridViewSelectionMode.FullRowSelect;
            dgvUser.MultiSelect           = false;
            dgvUser.AllowUserToAddRows    = false;
            dgvUser.AllowUserToDeleteRows = false;
            dgvUser.AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill;
            dgvUser.RowHeadersVisible     = false;
            dgvUser.BackgroundColor       = SystemColors.Window;
            dgvUser.BorderStyle           = BorderStyle.None;
            dgvUser.GridColor             = Color.FromArgb(220, 220, 220);
            dgvUser.ColumnHeadersHeightSizeMode =
                DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvUser.ColumnHeadersHeight   = 32;

            // Kolom
            dgvUser.Columns.Add(new DataGridViewTextBoxColumn
                { Name = "id_user", HeaderText = "ID", Visible = false });
            dgvUser.Columns.Add(new DataGridViewTextBoxColumn
                { Name = "nama_lengkap", HeaderText = "Nama Lengkap", FillWeight = 30 });
            dgvUser.Columns.Add(new DataGridViewTextBoxColumn
                { Name = "username", HeaderText = "Username", FillWeight = 20 });
            dgvUser.Columns.Add(new DataGridViewTextBoxColumn
                { Name = "role", HeaderText = "Role", FillWeight = 15 });
            dgvUser.Columns.Add(new DataGridViewTextBoxColumn
                { Name = "is_active", HeaderText = "Status", FillWeight = 12 });
            dgvUser.Columns.Add(new DataGridViewTextBoxColumn
                { Name = "dibuat_pada", HeaderText = "Dibuat", FillWeight = 23 });

            Controls.Add(dgvUser);
            Controls.Add(panelAtas);

            // Sambung event
            btnTambah.Click          += BtnTambah_Click;
            btnEdit.Click            += BtnEdit_Click;
            btnPassword.Click        += BtnPassword_Click;
            btnAktif.Click           += BtnAktif_Click;
            btnRefresh.Click         += (_, _) => MuatData();
            chkTampilNonaktif.CheckedChanged += (_, _) => MuatData();
            dgvUser.SelectionChanged += DgvUser_SelectionChanged;
            dgvUser.CellFormatting   += DgvUser_CellFormatting;

            SesuaikanLayout();
        }

        private static void KonfigurasiTombol(Button btn, string teks, Color warna)
        {
            btn.Text      = teks;
            btn.Height    = 32;
            btn.Width     = 130;
            btn.FlatStyle = FlatStyle.Flat;
            btn.BackColor = warna;
            btn.ForeColor = Color.White;
            btn.Cursor    = Cursors.Hand;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = ControlPaint.Light(warna, 0.15f);
        }

        private void SesuaikanLayout()
        {
            int x = 8;
            foreach (Button btn in new[] { btnTambah, btnEdit, btnPassword, btnAktif, btnRefresh })
            {
                btn.Left   = x;
                btn.Top    = 8;
                x         += btn.Width + 6;
            }
            chkTampilNonaktif.Left = x + 10;
            chkTampilNonaktif.Top  = 14;
        }

        // ─── LOAD DATA ───────────────────────────────────────────────

        private void FormUser_Load(object? sender, EventArgs e)
        {
            if (!Session.IsAdmin)
            {
                MessageBox.Show("Hanya Admin yang dapat mengakses halaman ini.",
                    "Akses Ditolak", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }
            MuatData();
        }

        private void MuatData()
        {
            dgvUser.Rows.Clear();

            string query = "SELECT id_user, nama_lengkap, username, role, is_active, dibuat_pada "
                         + "FROM tb_user ";
            if (!chkTampilNonaktif.Checked)
            {
                query += "WHERE is_active = 1 ";
            }
            query += "ORDER BY nama_lengkap";

            try
            {
                using SqliteConnection conn = _koneksi.GetConn();
                using SqliteCommand cmd = new(query, conn);
                using SqliteDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    int    idUser   = InputHelper.AmbilInt(reader["id_user"]);
                    string nama     = reader["nama_lengkap"]?.ToString() ?? "";
                    string username = reader["username"]?.ToString() ?? "";
                    string role     = reader["role"]?.ToString() ?? "";
                    bool   aktif    = InputHelper.AmbilDecimal(reader["is_active"]) == 1m;
                    string dibuat   = reader["dibuat_pada"]?.ToString() ?? "";

                    var baris = dgvUser.Rows.Add(idUser, nama, username, role, aktif ? "Aktif" : "Nonaktif", dibuat);

                    // Warnai baris nonaktif dengan abu-abu
                    if (!aktif)
                    {
                        dgvUser.Rows[baris].DefaultCellStyle.ForeColor = Color.Gray;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat data user:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            PerbaruitombolAktif();
        }

        // ─── TOMBOL TAMBAH ───────────────────────────────────────────

        private void BtnTambah_Click(object? sender, EventArgs e)
        {
            using var form = new FormUserEdit(null, _koneksi);
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                MuatData();
            }
        }

        // ─── TOMBOL EDIT ─────────────────────────────────────────────

        private void BtnEdit_Click(object? sender, EventArgs e)
        {
            if (!AmbilIdTerpilih(out int idUser)) return;

            using var form = new FormUserEdit(idUser, _koneksi);
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                MuatData();
            }
        }

        // ─── TOMBOL RESET PASSWORD ───────────────────────────────────

        private void BtnPassword_Click(object? sender, EventArgs e)
        {
            if (!AmbilIdTerpilih(out int idUser)) return;

            string? usernameTarget = dgvUser.SelectedRows[0].Cells[KolUsername].Value?.ToString();
            using var form = new FormResetPassword(idUser, usernameTarget ?? "", _koneksi);
            form.ShowDialog(this);
        }

        // ─── TOMBOL NONAKTIFKAN / AKTIFKAN ───────────────────────────

        private void BtnAktif_Click(object? sender, EventArgs e)
        {
            if (!AmbilIdTerpilih(out int idUser)) return;

            // Admin tidak bisa menonaktifkan dirinya sendiri.
            if (idUser == Session.UserId)
            {
                MessageBox.Show("Anda tidak bisa menonaktifkan akun Anda sendiri.",
                    "Tidak Diizinkan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool sedangAktif = dgvUser.SelectedRows[0]
                .Cells[KolAktif].Value?.ToString() == "Aktif";
            string aksi      = sedangAktif ? "nonaktifkan" : "aktifkan kembali";
            string nama      = dgvUser.SelectedRows[0].Cells[KolNama].Value?.ToString() ?? "";

            var konfirmasi = MessageBox.Show(
                $"Yakin ingin {aksi} akun \"{nama}\"?",
                "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (konfirmasi != DialogResult.Yes) return;

            try
            {
                using SqliteConnection conn = _koneksi.GetConn();
                using SqliteCommand cmd = new(
                    "UPDATE tb_user SET is_active = @aktif, "
                    + "diperbarui_pada = strftime('%Y-%m-%d %H:%M:%S','now','localtime') "
                    + "WHERE id_user = @id", conn);
                cmd.Parameters.AddWithValue("@aktif", sedangAktif ? 0 : 1);
                cmd.Parameters.AddWithValue("@id", idUser);
                cmd.ExecuteNonQuery();

                MuatData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal mengubah status:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ─── HELPER ─────────────────────────────────────────────────

        private bool AmbilIdTerpilih(out int idUser)
        {
            idUser = 0;
            if (dgvUser.SelectedRows.Count == 0)
            {
                MessageBox.Show("Pilih satu baris user terlebih dahulu.",
                    "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            idUser = InputHelper.AmbilInt(dgvUser.SelectedRows[0].Cells[KolId].Value);
            return true;
        }

        private void PerbaruitombolAktif()
        {
            bool adaPilihan = dgvUser.SelectedRows.Count > 0;
            btnEdit.Enabled     = adaPilihan;
            btnPassword.Enabled = adaPilihan;
            btnAktif.Enabled    = adaPilihan;

            if (adaPilihan)
            {
                bool sedangAktif = dgvUser.SelectedRows[0]
                    .Cells[KolAktif].Value?.ToString() == "Aktif";
                btnAktif.Text      = sedangAktif ? "⊘ Nonaktifkan" : "✓ Aktifkan";
                btnAktif.BackColor = sedangAktif
                    ? Color.FromArgb(231, 76, 60)
                    : Color.FromArgb(39, 174, 96);
            }
        }

        private void DgvUser_SelectionChanged(object? sender, EventArgs e) => PerbaruitombolAktif();

        private void DgvUser_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            // Tidak ada formatting khusus untuk saat ini.
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // Sub-form: FormUserEdit (tambah / edit user)
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Form untuk menambah atau mengedit data user.
    /// </summary>
    internal class FormUserEdit : Form
    {
        private readonly TextBox txtNama     = new();
        private readonly TextBox txtUsername = new();
        private readonly TextBox txtPassword = new();
        private readonly ComboBox cmbRole    = new();
        private readonly Button btnSimpan    = new();
        private readonly Button btnBatal     = new();
        private readonly Label lblPassword   = new();

        private readonly int? _idUser;
        private readonly Koneksi _koneksi;

        /// <param name="idUser">Null berarti mode tambah baru.</param>
        public FormUserEdit(int? idUser, Koneksi koneksi)
        {
            _idUser  = idUser;
            _koneksi = koneksi;

            Text            = idUser.HasValue ? "Edit User" : "Tambah User Baru";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            MinimizeBox     = false;
            StartPosition   = FormStartPosition.CenterParent;
            ClientSize      = new Size(380, 290);
            Font            = new Font("Segoe UI", 9F);

            BangunTampilan();
            Load += FormUserEdit_Load;
        }

        private void BangunTampilan()
        {
            var tabel = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                ColumnCount = 2,
                RowCount    = 6,
                Padding     = new Padding(14)
            };
            tabel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
            tabel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            for (int i = 0; i < 5; i++)
                tabel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tabel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            cmbRole.Items.AddRange(new object[] { "Admin", "Kasir" });
            cmbRole.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRole.Dock          = DockStyle.Fill;

            txtNama.Dock          = DockStyle.Fill;
            txtUsername.Dock      = DockStyle.Fill;
            txtPassword.Dock      = DockStyle.Fill;
            txtPassword.UseSystemPasswordChar = true;

            lblPassword.Text      = _idUser.HasValue ? "Password Baru" : "Password";
            lblPassword.TextAlign = ContentAlignment.MiddleLeft;
            lblPassword.Dock      = DockStyle.Fill;

            Tambah(tabel, "Nama Lengkap", txtNama, 0);
            Tambah(tabel, "Username",      txtUsername, 1);
            tabel.Controls.Add(lblPassword, 0, 2);
            tabel.Controls.Add(txtPassword, 1, 2);
            Tambah(tabel, "Role",          cmbRole, 3);

            if (_idUser.HasValue)
            {
                // Petunjuk: kosong = tidak ubah password
                var lblHint = new Label
                {
                    Text      = "Kosongkan jika tidak ingin mengubah password.",
                    Dock      = DockStyle.Fill,
                    ForeColor = Color.Gray,
                    Font      = new Font("Segoe UI", 7.5F)
                };
                tabel.Controls.Add(lblHint, 1, 4);
            }

            var panelTombol = new FlowLayoutPanel
            {
                Dock          = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Padding       = new Padding(0, 4, 0, 0)
            };

            KonfigTombol(btnSimpan, "Simpan", Color.FromArgb(39, 174, 96));
            KonfigTombol(btnBatal, "Batal", Color.FromArgb(150, 150, 150));
            panelTombol.Controls.AddRange(new Control[] { btnBatal, btnSimpan });
            tabel.Controls.Add(panelTombol, 0, 5);
            tabel.SetColumnSpan(panelTombol, 2);

            Controls.Add(tabel);

            btnSimpan.Click += BtnSimpan_Click;
            btnBatal.Click  += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
            AcceptButton     = btnSimpan;
        }

        private static void Tambah(TableLayoutPanel t, string label, Control ctrl, int baris)
        {
            t.Controls.Add(new Label
            {
                Text      = label,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock      = DockStyle.Fill
            }, 0, baris);
            t.Controls.Add(ctrl, 1, baris);
        }

        private static void KonfigTombol(Button btn, string teks, Color warna)
        {
            btn.Text      = teks;
            btn.Width     = 90;
            btn.Height    = 28;
            btn.FlatStyle = FlatStyle.Flat;
            btn.BackColor = warna;
            btn.ForeColor = Color.White;
            btn.FlatAppearance.BorderSize = 0;
        }

        private void FormUserEdit_Load(object? sender, EventArgs e)
        {
            if (!_idUser.HasValue)
            {
                cmbRole.SelectedIndex = 1; // default Kasir
                return;
            }

            try
            {
                using SqliteConnection conn = _koneksi.GetConn();
                using SqliteCommand cmd = new(
                    "SELECT nama_lengkap, username, role FROM tb_user WHERE id_user = @id", conn);
                cmd.Parameters.AddWithValue("@id", _idUser.Value);
                using SqliteDataReader r = cmd.ExecuteReader();
                if (r.Read())
                {
                    txtNama.Text     = r["nama_lengkap"]?.ToString() ?? "";
                    txtUsername.Text = r["username"]?.ToString() ?? "";
                    string role      = r["role"]?.ToString() ?? "Kasir";
                    cmbRole.SelectedItem = role;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat data:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSimpan_Click(object? sender, EventArgs e)
        {
            string nama     = txtNama.Text.Trim();
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;
            string role     = cmbRole.SelectedItem?.ToString() ?? "";

            if (nama.Length == 0 || username.Length == 0)
            {
                MessageBox.Show("Nama lengkap dan username tidak boleh kosong.",
                    "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!_idUser.HasValue && password.Length == 0)
            {
                MessageBox.Show("Password harus diisi untuk user baru.",
                    "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (password.Length > 0 && password.Length < 4)
            {
                MessageBox.Show("Password minimal 4 karakter.",
                    "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using SqliteConnection conn = _koneksi.GetConn();

                if (_idUser.HasValue)
                {
                    // Mode edit
                    string sql = "UPDATE tb_user SET nama_lengkap = @nama, username = @username, "
                               + "role = @role "
                               + (password.Length > 0 ? ", password = @password " : "")
                               + "WHERE id_user = @id";
                    using SqliteCommand cmd = new(sql, conn);
                    cmd.Parameters.AddWithValue("@nama", nama);
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@role", role);
                    cmd.Parameters.AddWithValue("@id", _idUser.Value);
                    if (password.Length > 0)
                        cmd.Parameters.AddWithValue("@password", PasswordHasher.Hash(password));
                    cmd.ExecuteNonQuery();
                }
                else
                {
                    // Mode tambah
                    using SqliteCommand cmd = new(
                        "INSERT INTO tb_user (nama_lengkap, username, password, role, is_active) "
                        + "VALUES (@nama, @username, @password, @role, 1)", conn);
                    cmd.Parameters.AddWithValue("@nama", nama);
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", PasswordHasher.Hash(password));
                    cmd.Parameters.AddWithValue("@role", role);
                    cmd.ExecuteNonQuery();
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (SqliteException ex) when (ex.Message.Contains("UNIQUE"))
            {
                MessageBox.Show("Username \"" + username + "\" sudah dipakai. Pilih username lain.",
                    "Duplikat", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal menyimpan:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // Sub-form: FormResetPassword
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Form khusus untuk mereset password user tanpa harus tahu password lama.
    /// Hanya bisa diakses oleh Admin.
    /// </summary>
    internal class FormResetPassword : Form
    {
        private readonly TextBox txtBaru   = new();
        private readonly TextBox txtUlangi = new();
        private readonly Button btnSimpan  = new();
        private readonly Button btnBatal   = new();

        private readonly int    _idUser;
        private readonly Koneksi _koneksi;

        public FormResetPassword(int idUser, string username, Koneksi koneksi)
        {
            _idUser  = idUser;
            _koneksi = koneksi;

            Text            = "Reset Password — " + username;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            MinimizeBox     = false;
            StartPosition   = FormStartPosition.CenterParent;
            ClientSize      = new Size(340, 200);
            Font            = new Font("Segoe UI", 9F);

            BangunTampilan();
        }

        private void BangunTampilan()
        {
            var tabel = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                ColumnCount = 2,
                RowCount    = 4,
                Padding     = new Padding(14)
            };
            tabel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
            tabel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            for (int i = 0; i < 3; i++)
                tabel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tabel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            txtBaru.Dock   = DockStyle.Fill;
            txtBaru.UseSystemPasswordChar = true;
            txtUlangi.Dock = DockStyle.Fill;
            txtUlangi.UseSystemPasswordChar = true;

            Tambah(tabel, "Password Baru", txtBaru,   0);
            Tambah(tabel, "Ulangi",        txtUlangi, 1);

            var panelTombol = new FlowLayoutPanel
            {
                Dock          = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Padding       = new Padding(0, 4, 0, 0)
            };
            Konfigurasi(btnSimpan, "Simpan", Color.FromArgb(39, 174, 96));
            Konfigurasi(btnBatal,  "Batal",  Color.FromArgb(150, 150, 150));
            panelTombol.Controls.AddRange(new Control[] { btnBatal, btnSimpan });
            tabel.Controls.Add(panelTombol, 0, 3);
            tabel.SetColumnSpan(panelTombol, 2);

            Controls.Add(tabel);

            btnSimpan.Click += BtnSimpan_Click;
            btnBatal.Click  += (_, _) => Close();
            AcceptButton     = btnSimpan;
        }

        private static void Tambah(TableLayoutPanel t, string label, Control ctrl, int baris)
        {
            t.Controls.Add(new Label
            {
                Text      = label,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock      = DockStyle.Fill
            }, 0, baris);
            t.Controls.Add(ctrl, 1, baris);
        }

        private static void Konfigurasi(Button btn, string teks, Color warna)
        {
            btn.Text      = teks;
            btn.Width     = 90;
            btn.Height    = 28;
            btn.FlatStyle = FlatStyle.Flat;
            btn.BackColor = warna;
            btn.ForeColor = Color.White;
            btn.FlatAppearance.BorderSize = 0;
        }

        private void BtnSimpan_Click(object? sender, EventArgs e)
        {
            string baru   = txtBaru.Text;
            string ulangi = txtUlangi.Text;

            if (baru.Length < 4)
            {
                MessageBox.Show("Password minimal 4 karakter.",
                    "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (baru != ulangi)
            {
                MessageBox.Show("Password dan konfirmasi tidak sama.",
                    "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUlangi.Clear();
                txtUlangi.Focus();
                return;
            }

            try
            {
                using SqliteConnection conn = _koneksi.GetConn();
                using SqliteCommand cmd = new(
                    "UPDATE tb_user SET password = @pwd WHERE id_user = @id", conn);
                cmd.Parameters.AddWithValue("@pwd", PasswordHasher.Hash(baru));
                cmd.Parameters.AddWithValue("@id", _idUser);
                cmd.ExecuteNonQuery();

                MessageBox.Show("Password berhasil direset.", "Berhasil",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal reset password:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
