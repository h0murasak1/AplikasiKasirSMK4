using Microsoft.Data.Sqlite;

namespace AplikasiKasirSMK4
{
    public partial class Form1 : Form
    {
        private readonly Koneksi _koneksi = new();
        private bool _sedangKeluar;

        public Form1()
        {
            InitializeComponent();
            UiThemeHelper.TerapkanIkon(this);
            FormClosing += Form1_FormClosing;

            txtUsername.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    if (txtPassword.Text.Length > 0)
                    {
                        btnLogin_Click(btnLogin, EventArgs.Empty);
                    }
                    else
                    {
                        txtPassword.Focus();
                    }
                }
            };

            txtPassword.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    btnLogin_Click(btnLogin, EventArgs.Empty);
                }
            };
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            UiThemeHelper.TerapkanIkon(this);

            // Muat logo sekolah ke PictureBox
            Image? logo = UiThemeHelper.AmbilLogoSekolah();
            if (logo != null)
            {
                picLogo.Image = logo;
            }

            // Skema database dibentuk otomatis saat pertama kali aplikasi
            // dijalankan, jadi tidak ada tombol "install" dan tidak perlu
            // menyiapkan apa pun lebih dulu. Kegagalan di sini berarti
            // folder aplikasi tidak bisa ditulis, dan itu memang perlu
            // diketahui sekarang, bukan saat kasir menekan tombol.
            try
            {
                Database.PastikanTerpasang(_koneksi);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Database belum bisa disiapkan.\n\n"
                    + "Lokasi berkas: " + _koneksi.PathDatabase + "\n\n"
                    + "Detail Error: " + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            if (!_koneksi.TestConnection(out string pesanError))
            {
                MessageBox.Show(
                    "Koneksi Gagal.\n\n"
                    + "Lokasi berkas: " + _koneksi.PathDatabase + "\n\n"
                    + "Detail Error: " + pesanError,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            txtUsername.Focus();
        }



        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            // Validasi input tidak boleh kosong
            if (username.Length == 0 || password.Length == 0)
            {
                MessageBox.Show(
                    "Username dan Password tidak boleh kosong!",
                    "Peringatan",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using SqliteConnection conn = _koneksi.GetConn();

                using SqliteCommand cmd = new(
                    "SELECT id_user, password, nama_lengkap, role, is_active "
                    + "FROM tb_user WHERE username = @username LIMIT 1",
                    conn);
                cmd.Parameters.AddWithValue("@username", username);

                int idUser;
                string passwordTersimpan;
                string namaLengkap;
                string role;
                bool akunAktif;

                using (SqliteDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        // Username tidak ditemukan. Tetap lanjut verifikasi dummy
                        // agar waktu respons tidak membocorkan keberadaan username.
                        PasswordHasher.Verify(password, PasswordHasher.Hash("dummy"));
                        TampilkanLoginGagal();
                        return;
                    }

                    idUser = InputHelper.AmbilInt(reader["id_user"]);
                    passwordTersimpan = reader["password"]?.ToString() ?? string.Empty;
                    namaLengkap = reader["nama_lengkap"]?.ToString() ?? username;
                    role = reader["role"]?.ToString() ?? string.Empty;
                    akunAktif = InputHelper.AmbilDecimal(reader["is_active"]) == 1m;
                }

                // Verifikasi password (mendukung hash baru dan password lama)
                if (!PasswordHasher.Verify(password, passwordTersimpan))
                {
                    TampilkanLoginGagal();
                    return;
                }

                // Password lama (plain text) otomatis di-upgrade menjadi hash.
                if (!PasswordHasher.IsHashed(passwordTersimpan))
                {
                    UpgradePasswordHash(conn, idUser, password);
                }

                // Akun yang dinonaktifkan admin tetap dianggap passwordnya benar,
                // tetapi tidak boleh masuk ke aplikasi.
                if (!akunAktif)
                {
                    MessageBox.Show(
                        "Akun \"" + username + "\" sudah dinonaktifkan oleh administrator.\n"
                        + "Hubungi administrator untuk mengaktifkannya kembali.",
                        "Akun Nonaktif",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                // Role yang tidak dikenal ditolak agar tidak ada akses tanpa hak.
                if (!SessionIsRoleValid(role))
                {
                    MessageBox.Show(
                        "Akun Anda tidak memiliki role yang valid. Hubungi administrator.",
                        "Akses Ditolak",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                Session.Set(idUser, username, namaLengkap, role);

                MessageBox.Show(
                    "Selamat datang, " + namaLengkap + "!",
                    "Login Berhasil",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                // Sembunyikan layar login lalu buka form sesuai role.
                Hide();

                if (Session.IsAdmin)
                {
                    new FormMenu().Show();
                }
                else
                {
                    new FormKasir { IsRootForm = true }.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Terjadi kesalahan sistem: " + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private static bool SessionIsRoleValid(string role)
        {
            return string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase)
                || string.Equals(role, "Kasir", StringComparison.OrdinalIgnoreCase);
        }

        private void TampilkanLoginGagal()
        {
            MessageBox.Show(
                "Username atau Password salah!",
                "Login Gagal",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            txtPassword.Clear();
            txtUsername.Focus();
            txtUsername.SelectAll();
        }

        /// <summary>
        /// Menyimpan ulang password dalam bentuk hash PBKDF2.
        /// Kegagalan upgrade tidak menggagalkan login.
        /// </summary>
        private static void UpgradePasswordHash(SqliteConnection conn, int idUser, string password)
        {
            try
            {
                using SqliteCommand cmd = new(
                    "UPDATE tb_user SET password = @password WHERE id_user = @id", conn);
                cmd.Parameters.AddWithValue("@password", PasswordHasher.Hash(password));
                cmd.Parameters.AddWithValue("@id", idUser);
                cmd.ExecuteNonQuery();
            }
            catch
            {
                // Tidak fatal: user tetap bisa login, coba lagi lain kali.
            }
        }

        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            // Bila form login ditutup tanpa login (mis. menekan tombol X
            // di taskbar), pastikan aplikasi benar-benar berhenti.
            if (!_sedangKeluar && !Session.IsLoggedIn)
            {
                _sedangKeluar = true;
                Application.Exit();
            }
        }
    }
}
