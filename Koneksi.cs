using System;
using System.Windows.Forms;
using MySql.Data.MySqlClient; // Memanggil library MySQL yang baru saja diinstal

namespace AplikasiKasirSMK4 // Sesuaikan dengan nama namespace/project Anda
{
    class Koneksi
    {
        // Pengaturan koneksi standar XAMPP (User: root, Password: kosong)
        string connectionString = "Server=localhost;Database=db_kasir_smk4;Uid=root;Pwd=;";
        MySqlConnection conn;

        // Fungsi utama untuk memanggil koneksi ke database
        public MySqlConnection GetConn()
        {
            conn = new MySqlConnection(connectionString);
            return conn;
        }

        // Fungsi khusus untuk mengetes apakah koneksi berhasil atau gagal
        public void CekKoneksi()
        {
            try
            {
                conn = new MySqlConnection(connectionString);
                conn.Open();
                MessageBox.Show("Koneksi ke Database MySQL Berhasil!", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Koneksi Gagal. Pastikan XAMPP menyala!\n\nDetail Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}