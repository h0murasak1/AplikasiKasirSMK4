using AplikasiKasirSMK4;
using MySql.Data.MySqlClient;

namespace AplikasiKasirSMK4
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Memanggil class Koneksi
            Koneksi koneksiDB = new Koneksi();

            // Menjalankan fungsi tes koneksi
            koneksiDB.CekKoneksi();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            // Cek apakah kolom kosong
            if (txtUsername.Text == "" || txtPassword.Text == "")
            {
                MessageBox.Show("Username dan Password tidak boleh kosong!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Memanggil class Koneksi
            Koneksi koneksiDB = new Koneksi();

            try
            {
                using (MySqlConnection conn = koneksiDB.GetConn())
                {
                    conn.Open();

                    // Query untuk mencocokkan data dengan tabel tb_user di MySQL
                    string query = "SELECT * FROM tb_user WHERE username = @username AND password = @password";
                    MySqlCommand cmd = new MySqlCommand(query, conn);

                    // Menggunakan Parameter untuk mencegah peretasan (SQL Injection)
                    cmd.Parameters.AddWithValue("@username", txtUsername.Text);
                    cmd.Parameters.AddWithValue("@password", txtPassword.Text);

                    MySqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows) // Jika data cocok dan ditemukan di database
                    {
                        reader.Read();
                        string namaLengkap = reader["nama_lengkap"].ToString();
                        string hakAkses = reader["role"].ToString();

                        MessageBox.Show("Selamat datang, " + namaLengkap + "!",
                                        "Login Berhasil", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // CEK ROLE (Hak Akses)
                        if (hakAkses == "Admin")
                        {
                            FormMenu menuAdmin = new FormMenu();
                            menuAdmin.Show();
                        }
                        else if (hakAkses == "Kasir")
                        {
                            FormKasir mejaKasir = new FormKasir();
                            mejaKasir.Show();
                        }

                        this.Hide(); // Sembunyikan layar login
                    }
                    else
                    {
                        MessageBox.Show("Username atau Password salah!", "Login Gagal", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        txtPassword.Clear();
                        txtUsername.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Terjadi kesalahan sistem: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {

        }
    }
}
