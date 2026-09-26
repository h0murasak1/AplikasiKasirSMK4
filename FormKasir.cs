using System;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace AplikasiKasirSMK4
{
    public partial class FormKasir : Form
    {
        // Memanggil class Koneksi Database
        Koneksi koneksiDB = new Koneksi();

        public FormKasir()
        {
            InitializeComponent();
        }

        // =======================================================
        // 1. EVENT BARCODE SCANNER
        // =======================================================
        private void txtBarcode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (txtBarcode.Text != "")
                {
                    CariDanMasukKeranjang(txtBarcode.Text);
                    txtBarcode.Text = ""; // Kosongkan kolom setelah di-scan
                }
            }
        }

        private void CariDanMasukKeranjang(string kode)
        {
            try
            {
                using (MySqlConnection conn = koneksiDB.GetConn())
                {
                    conn.Open();
                    string query = "SELECT * FROM tb_barang WHERE kode_barcode = @kode";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@kode", kode);

                    MySqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        reader.Read();
                        string nama = reader["nama_barang"].ToString();
                        int harga = Convert.ToInt32(reader["harga_jual"]);

                        bool barangSudahAda = false;
                        foreach (DataGridViewRow row in dgvKeranjang.Rows)
                        {
                            if (row.Cells[0].Value != null && row.Cells[0].Value.ToString() == kode)
                            {
                                int qtyLama = Convert.ToInt32(row.Cells[3].Value);
                                row.Cells[3].Value = qtyLama + 1;
                                row.Cells[4].Value = (qtyLama + 1) * harga;
                                barangSudahAda = true;
                                break;
                            }
                        }

                        if (!barangSudahAda)
                        {
                            dgvKeranjang.Rows.Add(kode, nama, harga, 1, harga);
                        }

                        HitungTotalBelanja();
                    }
                    else
                    {
                        MessageBox.Show("Barang tidak ditemukan di database!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void HitungTotalBelanja()
        {
            int total = 0;
            foreach (DataGridViewRow row in dgvKeranjang.Rows)
            {
                if (row.Cells[4].Value != null)
                {
                    total += Convert.ToInt32(row.Cells[4].Value);
                }
            }
            lblTotal.Text = total.ToString();
        }

        // =======================================================
        // 2. EVENT PENGHITUNG KEMBALIAN OTOMATIS
        // =======================================================
        private void txtBayar_TextChanged(object sender, EventArgs e)
        {
            if (txtBayar.Text != "" && lblTotal.Text != "0")
            {
                try
                {
                    int total = Convert.ToInt32(lblTotal.Text);
                    int uangBayar = Convert.ToInt32(txtBayar.Text);
                    int kembalian = uangBayar - total;

                    lblKembalian.Text = kembalian.ToString();
                }
                catch
                {
                    // Abaikan jika huruf yang diketik (bukan angka)
                }
            }
            else
            {
                lblKembalian.Text = "0";
            }
        }

        // =======================================================
        // 3. EVENT TOMBOL BAYAR & SIMPAN
        // =======================================================
        private void btnBayar_Click(object sender, EventArgs e)
        {
            if (dgvKeranjang.Rows.Count == 0)
            {
                MessageBox.Show("Keranjang belanja masih kosong!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int totalBelanja = Convert.ToInt32(lblTotal.Text);
            int uangBayar = txtBayar.Text == "" ? 0 : Convert.ToInt32(txtBayar.Text);

            if (uangBayar < totalBelanja)
            {
                MessageBox.Show("Uang pembayaran kurang!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                using (MySqlConnection conn = koneksiDB.GetConn())
                {
                    conn.Open();

                    // Format Nomor Nota: TRX-TahunBulanTanggalJamMenitDetik
                    string noNota = "TRX-" + DateTime.Now.ToString("yyyyMMddHHmmss");

                    // Simpan ke Tabel Transaksi
                    string queryTransaksi = "INSERT INTO tb_transaksi (no_nota, id_user, total_bayar) VALUES (@noNota, 1, @totalBayar)";
                    MySqlCommand cmdTrans = new MySqlCommand(queryTransaksi, conn);
                    cmdTrans.Parameters.AddWithValue("@noNota", noNota);
                    cmdTrans.Parameters.AddWithValue("@totalBayar", totalBelanja);
                    cmdTrans.ExecuteNonQuery();

                    // Simpan Detail & Kurangi Stok
                    foreach (DataGridViewRow row in dgvKeranjang.Rows)
                    {
                        if (row.Cells[0].Value != null)
                        {
                            string kodeBarcode = row.Cells[0].Value.ToString();
                            int qty = Convert.ToInt32(row.Cells[3].Value);
                            int subtotal = Convert.ToInt32(row.Cells[4].Value);

                            string queryDetail = "INSERT INTO tb_detail_transaksi (no_nota, kode_barcode, qty, subtotal) VALUES (@noNota, @kode, @qty, @subtotal)";
                            MySqlCommand cmdDetail = new MySqlCommand(queryDetail, conn);
                            cmdDetail.Parameters.AddWithValue("@noNota", noNota);
                            cmdDetail.Parameters.AddWithValue("@kode", kodeBarcode);
                            cmdDetail.Parameters.AddWithValue("@qty", qty);
                            cmdDetail.Parameters.AddWithValue("@subtotal", subtotal);
                            cmdDetail.ExecuteNonQuery();

                            string queryStok = "UPDATE tb_barang SET stok = stok - @qty WHERE kode_barcode = @kode";
                            MySqlCommand cmdStok = new MySqlCommand(queryStok, conn);
                            cmdStok.Parameters.AddWithValue("@qty", qty);
                            cmdStok.Parameters.AddWithValue("@kode", kodeBarcode);
                            cmdStok.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("Transaksi Berhasil Disimpan!\nNomor Nota: " + noNota + "\nKembalian: Rp " + lblKembalian.Text, "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Bersihkan layar untuk transaksi berikutnya
                    dgvKeranjang.Rows.Clear();
                    lblTotal.Text = "0";
                    txtBayar.Text = "";
                    lblKembalian.Text = "0";
                    txtBarcode.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal menyimpan transaksi: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}