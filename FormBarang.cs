using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace AplikasiKasirSMK4
{
    public partial class FormBarang : Form
    {
        Koneksi koneksiDB = new Koneksi();

        public FormBarang()
        {
            InitializeComponent();
        }

        private void FormBarang_Load(object sender, EventArgs e)
        {
            TampilData();
        }

        // Fungsi (READ) - Memanggil data dari database ke Tabel
        private void TampilData()
        {
            try
            {
                using (MySqlConnection conn = koneksiDB.GetConn())
                {
                    conn.Open();
                    string query = "SELECT kode_barcode AS 'Kode', nama_barang AS 'Nama Barang', harga_beli AS 'Harga Beli', harga_jual AS 'Harga Jual', stok AS 'Stok' FROM tb_barang";
                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dgvBarang.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Fungsi Membersihkan Kolom Input
        private void BersihkanForm()
        {
            txtKode.Text = "";
            txtNama.Text = "";
            txtHargaBeli.Text = "";
            txtHargaJual.Text = "";
            txtStok.Text = "";
            txtKode.Enabled = true; // <-- Ini yang akan membuka kunci barcode
            txtKode.Focus();
        }

        private void btnBersihkan_Click(object sender, EventArgs e)
        {
            BersihkanForm();
        }

        // Fungsi (CREATE) - Menyimpan Barang Baru
        private void btnSimpan_Click(object sender, EventArgs e)
        {
            if (txtKode.Text == "" || txtNama.Text == "" || txtHargaJual.Text == "" || txtStok.Text == "")
            {
                MessageBox.Show("Mohon lengkapi semua data!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (MySqlConnection conn = koneksiDB.GetConn())
                {
                    conn.Open();
                    string query = "INSERT INTO tb_barang (kode_barcode, nama_barang, harga_beli, harga_jual, stok) VALUES (@kode, @nama, @hargabeli, @hargajual, @stok)";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@kode", txtKode.Text);
                    cmd.Parameters.AddWithValue("@nama", txtNama.Text);
                    cmd.Parameters.AddWithValue("@hargabeli", txtHargaBeli.Text == "" ? "0" : txtHargaBeli.Text);
                    cmd.Parameters.AddWithValue("@hargajual", txtHargaJual.Text);
                    cmd.Parameters.AddWithValue("@stok", txtStok.Text);

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Data Barang berhasil ditambahkan!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    BersihkanForm();
                    TampilData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal menyimpan data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Memindahkan data dari Tabel ke Kolom Input saat diklik
        private void dgvBarang_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvBarang.Rows[e.RowIndex];
                txtKode.Text = row.Cells["Kode"].Value.ToString();
                txtNama.Text = row.Cells["Nama Barang"].Value.ToString();
                txtHargaBeli.Text = row.Cells["Harga Beli"].Value.ToString();
                txtHargaJual.Text = row.Cells["Harga Jual"].Value.ToString();
                txtStok.Text = row.Cells["Stok"].Value.ToString();

                // Mencegah kode barcode diubah saat mode edit
                txtKode.Enabled = false;
            }
        }

        // Fungsi (UPDATE) - Memperbarui Data Barang
        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (txtKode.Text == "")
            {
                MessageBox.Show("Pilih data yang ingin diperbarui dari tabel!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (MySqlConnection conn = koneksiDB.GetConn())
                {
                    conn.Open();
                    string query = "UPDATE tb_barang SET nama_barang = @nama, harga_beli = @hargabeli, harga_jual = @hargajual, stok = @stok WHERE kode_barcode = @kode";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@nama", txtNama.Text);
                    cmd.Parameters.AddWithValue("@hargabeli", txtHargaBeli.Text);
                    cmd.Parameters.AddWithValue("@hargajual", txtHargaJual.Text);
                    cmd.Parameters.AddWithValue("@stok", txtStok.Text);
                    cmd.Parameters.AddWithValue("@kode", txtKode.Text);

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Data Barang berhasil diperbarui!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    txtKode.Enabled = true; // Buka kembali kolom input kode
                    BersihkanForm();
                    TampilData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memperbarui data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Fungsi (DELETE) - Menghapus Data Barang
        private void btnHapus_Click(object sender, EventArgs e)
        {
            if (txtKode.Text == "")
            {
                MessageBox.Show("Pilih data yang ingin dihapus dari tabel!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult dialogResult = MessageBox.Show("Apakah Anda yakin ingin menghapus barang " + txtNama.Text + "?", "Konfirmasi Hapus", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (dialogResult == DialogResult.Yes)
            {
                try
                {
                    using (MySqlConnection conn = koneksiDB.GetConn())
                    {
                        conn.Open();
                        string query = "DELETE FROM tb_barang WHERE kode_barcode = @kode";
                        MySqlCommand cmd = new MySqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@kode", txtKode.Text);

                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Data berhasil dihapus!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        txtKode.Enabled = true;
                        BersihkanForm();
                        TampilData();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal menghapus data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}