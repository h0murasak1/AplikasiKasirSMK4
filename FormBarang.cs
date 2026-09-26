using System.Data;
using MySql.Data.MySqlClient;

namespace AplikasiKasirSMK4
{
    public partial class FormBarang : Form
    {
        private readonly Koneksi _koneksi = new();

        public FormBarang()
        {
            InitializeComponent();
            dgvBarang.CellFormatting += dgvBarang_CellFormatting;
        }

        private void FormBarang_Load(object sender, EventArgs e)
        {
            // Guard: master barang hanya boleh diakses Admin.
            if (!Session.IsAdmin)
            {
                MessageBox.Show(
                    "Hanya akun Admin yang dapat mengelola data barang.",
                    "Akses Ditolak",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                Close();
                return;
            }

            TampilData();
        }

        // ------------------------------------------------------------------
        // READ
        // ------------------------------------------------------------------
        private void TampilData()
        {
            try
            {
                using MySqlConnection conn = _koneksi.GetConn();
                conn.Open();

                const string query =
                    "SELECT kode_barcode AS 'Kode', nama_barang AS 'Nama Barang', "
                    + "harga_beli AS 'Harga Beli', harga_jual AS 'Harga Jual', "
                    + "stok AS 'Stok' FROM tb_barang ORDER BY nama_barang ASC";

                using MySqlDataAdapter adapter = new(query, conn);
                DataTable dt = new();
                adapter.Fill(dt);
                dgvBarang.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Gagal memuat data: " + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // ------------------------------------------------------------------
        // Helper
        // ------------------------------------------------------------------
        private void BersihkanForm()
        {
            txtKode.Text = string.Empty;
            txtNama.Text = string.Empty;
            txtHargaBeli.Text = string.Empty;
            txtHargaJual.Text = string.Empty;
            txtStok.Text = string.Empty;
            txtKode.Enabled = true;   // buka kembali kunci barcode
            txtKode.Focus();
        }

        private bool AmbilInput(out string kode, out string nama, out decimal hargaBeli,
            out decimal hargaJual, out int stok)
        {
            kode = txtKode.Text.Trim();
            nama = txtNama.Text.Trim();
            hargaBeli = 0m;
            hargaJual = 0m;
            stok = 0;

            if (kode.Length == 0)
            {
                MessageBox.Show("Kode barcode tidak boleh kosong!", "Peringatan",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtKode.Focus();
                return false;
            }

            if (nama.Length == 0)
            {
                MessageBox.Show("Nama barang tidak boleh kosong!", "Peringatan",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNama.Focus();
                return false;
            }

            if (!InputHelper.TryParseNominal(txtHargaJual.Text, out hargaJual, out string pesanJual))
            {
                MessageBox.Show("Harga jual tidak valid: " + pesanJual + ".", "Peringatan",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtHargaJual.Focus();
                return false;
            }

            if (hargaJual <= 0m)
            {
                MessageBox.Show("Harga jual harus lebih besar dari 0!", "Peringatan",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtHargaJual.Focus();
                return false;
            }

            // Harga beli opsional, default 0
            if (txtHargaBeli.Text.Trim().Length == 0)
            {
                hargaBeli = 0m;
            }
            else if (!InputHelper.TryParseNominal(txtHargaBeli.Text, out hargaBeli, out string pesanBeli))
            {
                MessageBox.Show("Harga beli tidak valid: " + pesanBeli + ".", "Peringatan",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtHargaBeli.Focus();
                return false;
            }

            if (!InputHelper.TryParseNominal(txtStok.Text, out decimal stokTemp, out string pesanStok))
            {
                MessageBox.Show("Jumlah stok tidak valid: " + pesanStok + ".", "Peringatan",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtStok.Focus();
                return false;
            }

            if (stokTemp != decimal.Truncate(stokTemp))
            {
                MessageBox.Show("Jumlah stok harus berupa bilangan bulat!", "Peringatan",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtStok.Focus();
                return false;
            }

            if (stokTemp > int.MaxValue)
            {
                MessageBox.Show("Jumlah stok terlalu besar!", "Peringatan",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtStok.Focus();
                return false;
            }

            stok = (int)stokTemp;
            return true;
        }

        private static bool KodeSudahAda(MySqlConnection conn, string kode)
        {
            const string query = "SELECT COUNT(*) FROM tb_barang WHERE kode_barcode = @kode";

            using MySqlCommand cmd = new(query, conn);
            cmd.Parameters.AddWithValue("@kode", kode);

            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        // ------------------------------------------------------------------
        // CREATE
        // ------------------------------------------------------------------
        private void btnSimpan_Click(object sender, EventArgs e)
        {
            if (!AmbilInput(out string kode, out string nama, out decimal hargaBeli,
                    out decimal hargaJual, out int stok))
            {
                return;
            }

            try
            {
                using MySqlConnection conn = _koneksi.GetConn();
                conn.Open();

                // Cegah kode barcode duplikat dengan pesan yang jelas.
                if (KodeSudahAda(conn, kode))
                {
                    MessageBox.Show(
                        "Kode barcode \"" + kode + "\" sudah dipakai barang lain.\n"
                        + "Gunakan kode lain atau klik baris barang untuk mengubahnya.",
                        "Peringatan",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    txtKode.Focus();
                    txtKode.SelectAll();
                    return;
                }

                const string query =
                    "INSERT INTO tb_barang "
                    + "(kode_barcode, nama_barang, harga_beli, harga_jual, stok) "
                    + "VALUES (@kode, @nama, @hargaBeli, @hargaJual, @stok)";

                using MySqlCommand cmd = new(query, conn);
                cmd.Parameters.AddWithValue("@kode", kode);
                cmd.Parameters.AddWithValue("@nama", nama);
                cmd.Parameters.AddWithValue("@hargaBeli", hargaBeli);
                cmd.Parameters.AddWithValue("@hargaJual", hargaJual);
                cmd.Parameters.AddWithValue("@stok", stok);
                cmd.ExecuteNonQuery();

                MessageBox.Show("Data barang berhasil ditambahkan!", "Sukses",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                BersihkanForm();
                TampilData();
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                MessageBox.Show(
                    "Kode barcode \"" + kode + "\" sudah dipakai barang lain.",
                    "Peringatan",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal menyimpan data: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ------------------------------------------------------------------
        // Isi form dari tabel
        // ------------------------------------------------------------------
        private void dgvBarang_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            DataGridViewRow row = dgvBarang.Rows[e.RowIndex];
            if (row.DataBoundItem is not DataRowView data)
            {
                return;
            }

            txtKode.Text = Convert.ToString(data["Kode"]) ?? string.Empty;
            txtNama.Text = Convert.ToString(data["Nama Barang"]) ?? string.Empty;
            txtHargaBeli.Text = FormatNilaiKoma(data["Harga Beli"]);
            txtHargaJual.Text = FormatNilaiKoma(data["Harga Jual"]);
            txtStok.Text = FormatNilaiKoma(data["Stok"]);

            // Kode barcode dikunci saat mode edit.
            txtKode.Enabled = false;
            txtNama.Focus();
        }

        private static string FormatNilaiKoma(object? nilai)
        {
            return nilai switch
            {
                null => string.Empty,
                decimal d => d.ToString("0.##"),
                int i => i.ToString(),
                _ => nilai.ToString() ?? string.Empty
            };
        }

        // ------------------------------------------------------------------
        // UPDATE
        // ------------------------------------------------------------------
        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (txtKode.Text.Trim().Length == 0)
            {
                MessageBox.Show(
                    "Pilih data yang ingin diperbarui dari tabel!",
                    "Peringatan",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (!AmbilInput(out string kode, out string nama, out decimal hargaBeli,
                    out decimal hargaJual, out int stok))
            {
                return;
            }

            try
            {
                using MySqlConnection conn = _koneksi.GetConn();
                conn.Open();

                const string query =
                    "UPDATE tb_barang SET nama_barang = @nama, harga_beli = @hargaBeli, "
                    + "harga_jual = @hargaJual, stok = @stok WHERE kode_barcode = @kode";

                using MySqlCommand cmd = new(query, conn);
                cmd.Parameters.AddWithValue("@nama", nama);
                cmd.Parameters.AddWithValue("@hargaBeli", hargaBeli);
                cmd.Parameters.AddWithValue("@hargaJual", hargaJual);
                cmd.Parameters.AddWithValue("@stok", stok);
                cmd.Parameters.AddWithValue("@kode", kode);

                int affected = cmd.ExecuteNonQuery();
                if (affected == 0)
                {
                    MessageBox.Show(
                        "Data tidak ditemukan. Silakan pilih ulang dari tabel.",
                        "Peringatan",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
                else
                {
                    MessageBox.Show("Data barang berhasil diperbarui!", "Sukses",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                txtKode.Enabled = true;
                BersihkanForm();
                TampilData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memperbarui data: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ------------------------------------------------------------------
        // DELETE
        // ------------------------------------------------------------------
        private void btnHapus_Click(object sender, EventArgs e)
        {
            if (txtKode.Text.Trim().Length == 0)
            {
                MessageBox.Show(
                    "Pilih data yang ingin dihapus dari tabel!",
                    "Peringatan",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            DialogResult konfirmasi = MessageBox.Show(
                "Apakah Anda yakin ingin menghapus barang \"" + txtNama.Text + "\"?\n"
                + "Data transaksi lama tidak ikut terhapus.",
                "Konfirmasi Hapus",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (konfirmasi != DialogResult.Yes)
            {
                return;
            }

            try
            {
                using MySqlConnection conn = _koneksi.GetConn();
                conn.Open();

                const string query = "DELETE FROM tb_barang WHERE kode_barcode = @kode";
                using MySqlCommand cmd = new(query, conn);
                cmd.Parameters.AddWithValue("@kode", txtKode.Text.Trim());
                cmd.ExecuteNonQuery();

                MessageBox.Show("Data berhasil dihapus!", "Sukses",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                txtKode.Enabled = true;
                BersihkanForm();
                TampilData();
            }
            catch (MySqlException ex) when (ex.Number == 1451)
            {
                // Foreign key constraint: barang masih dipakai di transaksi.
                MessageBox.Show(
                    "Barang tidak bisa dihapus karena sudah dipakai pada transaksi sebelumnya.",
                    "Peringatan",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal menghapus data: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnBersihkan_Click(object sender, EventArgs e)
        {
            BersihkanForm();
        }

        // ------------------------------------------------------------------
        // Tampilan angka pada tabel
        // ------------------------------------------------------------------
        private void dgvBarang_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            // KolomIndex = -1 terjadi saat event dipanggil untuk row header.
            if (e.ColumnIndex < 0 || e.RowIndex < 0)
            {
                return;
            }

            if (dgvBarang.Columns[e.ColumnIndex].Name is not ("Harga Beli" or "Harga Jual" or "Stok"))
            {
                return;
            }

            object? nilai = dgvBarang.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
            if (nilai is null)
            {
                return;
            }

            decimal angka = InputHelper.AmbilDecimal(nilai);
            e.Value = InputHelper.FormatNominal(angka);
            e.FormattingApplied = true;
        }
    }
}
