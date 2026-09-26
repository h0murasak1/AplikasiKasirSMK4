using System.ComponentModel;
using MySql.Data.MySqlClient;

namespace AplikasiKasirSMK4
{
    public partial class FormKasir : Form
    {
        private readonly Koneksi _koneksi = new();
        private decimal _totalBelanja;
        private bool _sedangKeluar;

        /// <summary>
        /// True bila form ini adalah form utama yang dibuka langsung setelah login
        /// (role Kasir). Bila form ditutup, aplikasi ikut berhenti.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsRootForm { get; set; }

        public FormKasir()
        {
            InitializeComponent();
            Load += FormKasir_Load;
            FormClosing += FormKasir_FormClosing;
            dgvKeranjang.CellFormatting += dgvKeranjang_CellFormatting;
            dgvKeranjang.CellDoubleClick += dgvKeranjang_CellDoubleClick;
        }

        private void FormKasir_Load(object? sender, EventArgs e)
        {
            if (!Session.IsLoggedIn)
            {
                MessageBox.Show(
                    "Silakan login terlebih dahulu.",
                    "Akses Ditolak",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                Close();
                return;
            }

            ResetTransaksi();
            txtBarcode.Focus();
        }

        // =========================================================
        // 1. SCAN BARCODE
        // =========================================================
        private void txtBarcode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            e.SuppressKeyPress = true;   // mencegah bunyi "ding" beep

            string kode = txtBarcode.Text.Trim();
            if (kode.Length == 0)
            {
                return;
            }

            CariDanMasukKeranjang(kode);
        }

        private void CariDanMasukKeranjang(string kode)
        {
            string nama;
            decimal harga;
            int stokTersedia;

            try
            {
                using MySqlConnection conn = _koneksi.GetConn();
                conn.Open();

                // Tidak memakai SELECT * agar tidak bergantung urutan kolom.
                const string query =
                    "SELECT nama_barang, harga_jual, stok FROM tb_barang "
                    + "WHERE kode_barcode = @kode LIMIT 1";

                using MySqlCommand cmd = new(query, conn);
                cmd.Parameters.AddWithValue("@kode", kode);

                using MySqlDataReader reader = cmd.ExecuteReader();
                if (!reader.Read())
                {
                    TampilkanPeringatanBarang(kode);
                    return;
                }

                nama = reader["nama_barang"]?.ToString() ?? string.Empty;
                harga = InputHelper.AmbilDecimal(reader["harga_jual"]);
                stokTersedia = InputHelper.AmbilInt(reader["stok"]);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal mencari barang: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (harga <= 0m)
            {
                TampilkanPeringatan("Barang \"" + nama + "\" tidak memiliki harga jual yang valid.");
                return;
            }

            int qtyTersedia = AmbilQtyKeranjang(kode);

            // Validasi stok: barang dengan stok 0 tidak boleh masuk keranjang,
            // dan total qty tidak boleh melebihi stok tersedia.
            if (stokTersedia <= 0)
            {
                TampilkanPeringatan("Stok barang \"" + nama + "\" habis!");
                return;
            }

            if (qtyTersedia + 1 > stokTersedia)
            {
                TampilkanPeringatan(
                    "Stok \"" + nama + "\" tidak cukup!\n"
                    + "Stok tersedia: " + stokTersedia
                    + ", sudah di keranjang: " + qtyTersedia + ".");
                return;
            }

            if (qtyTersedia > 0)
            {
                // Barang sudah ada di keranjang -> tambahkan qty saja.
                PerbaruiBarisKeranjang(kode, qtyTersedia + 1);
            }
            else
            {
                dgvKeranjang.Rows.Add(kode, nama, harga, 1, harga);
            }

            HitungTotalBelanja();
            txtBarcode.Text = string.Empty;
            txtBarcode.Focus();
        }

        private void TampilkanPeringatanBarang(string kode)
        {
            TampilkanPeringatan("Barang dengan kode \"" + kode + "\" tidak ditemukan!");
        }

        private static void TampilkanPeringatan(string pesan)
        {
            MessageBox.Show(pesan, "Peringatan",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        /// <summary>Mengembalikan qty barang tertentu yang sudah ada di keranjang.</summary>
        private int AmbilQtyKeranjang(string kode)
        {
            foreach (DataGridViewRow row in dgvKeranjang.Rows)
            {
                if (row.IsNewRow)
                {
                    continue;
                }

                if (string.Equals(row.Cells[0].Value?.ToString(), kode,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return InputHelper.AmbilInt(row.Cells[3].Value);
                }
            }

            return 0;
        }

        private void PerbaruiBarisKeranjang(string kode, int qtyBaru)
        {
            foreach (DataGridViewRow row in dgvKeranjang.Rows)
            {
                if (row.IsNewRow)
                {
                    continue;
                }

                if (!string.Equals(row.Cells[0].Value?.ToString(), kode,
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                decimal harga = InputHelper.AmbilDecimal(row.Cells[2].Value);
                row.Cells[3].Value = qtyBaru;
                row.Cells[4].Value = harga * qtyBaru;
                dgvKeranjang.Refresh();
                return;
            }
        }

        // =========================================================
        // 2. HAPUS ITEM (double click pada baris keranjang)
        // =========================================================
        private void dgvKeranjang_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            DataGridViewRow row = dgvKeranjang.Rows[e.RowIndex];
            if (row.IsNewRow)
            {
                return;
            }

            string nama = row.Cells[1].Value?.ToString() ?? "(tanpa nama)";

            DialogResult konfirmasi = MessageBox.Show(
                "Hapus \"" + nama + "\" dari keranjang?",
                "Konfirmasi Hapus Item",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (konfirmasi != DialogResult.Yes)
            {
                return;
            }

            dgvKeranjang.Rows.Remove(row);
            HitungTotalBelanja();
            txtBarcode.Focus();
        }

        // =========================================================
        // 3. TOTAL DAN KEMBALIAN
        // =========================================================
        private void HitungTotalBelanja()
        {
            decimal total = 0m;

            foreach (DataGridViewRow row in dgvKeranjang.Rows)
            {
                if (row.IsNewRow)
                {
                    continue;
                }

                total += InputHelper.AmbilDecimal(row.Cells[4].Value);
            }

            _totalBelanja = total;
            lblTotal.Text = InputHelper.FormatNominal(total);

            // Hitung ulang kembalian agar tetap konsisten.
            PerbaruiKembalian();
        }

        private void txtBayar_TextChanged(object sender, EventArgs e)
        {
            PerbaruiKembalian();
        }

        private void PerbaruiKembalian()
        {
            if (!InputHelper.TryParseNominal(txtBayar.Text, out decimal uangBayar, out _)
                || _totalBelanja <= 0m)
            {
                lblKembalian.Text = "0";
                return;
            }

            decimal kembalian = uangBayar - _totalBelanja;

            // Jangan tampilkan angka negatif.
            lblKembalian.Text = kembalian > 0m
                ? InputHelper.FormatNominal(kembalian)
                : "0";
        }

        // =========================================================
        // 4. BAYAR DAN SIMPAN
        // =========================================================
        private void btnBayar_Click(object sender, EventArgs e)
        {
            if (dgvKeranjang.Rows.Count == 0)
            {
                TampilkanPeringatan("Keranjang belanja masih kosong!");
                return;
            }

            if (!Session.IsLoggedIn)
            {
                TampilkanPeringatan("Session berakhir. Silakan login kembali.");
                return;
            }

            if (!InputHelper.TryParseNominal(txtBayar.Text, out decimal uangBayar, out string pesanBayar)
                || uangBayar <= 0m)
            {
                TampilkanPeringatan("Uang pembayaran tidak valid: "
                    + (pesanBayar.Length > 0 ? pesanBayar : "nilai harus lebih besar dari 0") + ".");
                return;
            }

            if (uangBayar < _totalBelanja)
            {
                TampilkanPeringatan(
                    "Uang pembayaran kurang!\n"
                    + "Total belanja: Rp " + InputHelper.FormatNominal(_totalBelanja)
                    + "\nUang dibayar: Rp " + InputHelper.FormatNominal(uangBayar));
                return;
            }

            decimal kembalian = uangBayar - _totalBelanja;

            try
            {
                string noNota = SimpanTransaksi();

                MessageBox.Show(
                    "Transaksi Berhasil Disimpan!\n\n"
                    + "Nomor Nota : " + noNota + "\n"
                    + "Total      : Rp " + InputHelper.FormatNominal(_totalBelanja) + "\n"
                    + "Dibayar    : Rp " + InputHelper.FormatNominal(uangBayar) + "\n"
                    + "Kembalian  : Rp " + InputHelper.FormatNominal(kembalian),
                    "Sukses",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                ResetTransaksi();
            }
            catch (StokTidakCukupException ex)
            {
                MessageBox.Show(
                    ex.Message + "\n\nTransaksi dibatalkan, tidak ada data yang tersimpan.",
                    "Stok Tidak Cukup",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Gagal menyimpan transaksi: " + ex.Message
                    + "\n\nTransaksi dibatalkan, tidak ada data yang tersimpan.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Menyimpan transaksi di dalam database transaction.
        /// Semua perubahan dibatalkan (rollback) bila terjadi satu saja kegagalan.
        /// </summary>
        private string SimpanTransaksi()
        {
            using MySqlConnection conn = _koneksi.GetConn();
            conn.Open();

            using MySqlTransaction transaksi = conn.BeginTransaction();

            try
            {
                // id_user diambil dari session, bukan angka hardcode.
                int idUser = Session.UserId;

                string noNota = BuatNomorNota();

                const string queryTransaksi =
                    "INSERT INTO tb_transaksi (no_nota, id_user, total_bayar) "
                    + "VALUES (@noNota, @idUser, @totalBayar)";

                using (MySqlCommand cmdTrans = new(queryTransaksi, conn, transaksi))
                {
                    cmdTrans.Parameters.AddWithValue("@noNota", noNota);
                    cmdTrans.Parameters.AddWithValue("@idUser", idUser);
                    cmdTrans.Parameters.AddWithValue("@totalBayar", _totalBelanja);
                    cmdTrans.ExecuteNonQuery();
                }

                const string queryDetail =
                    "INSERT INTO tb_detail_transaksi (no_nota, kode_barcode, qty, subtotal) "
                    + "VALUES (@noNota, @kode, @qty, @subtotal)";

                const string queryStok =
                    "UPDATE tb_barang SET stok = stok - @qty WHERE kode_barcode = @kode";

                const string queryCekStok =
                    "SELECT nama_barang, stok FROM tb_barang "
                    + "WHERE kode_barcode = @kode FOR UPDATE";

                foreach (DataGridViewRow row in dgvKeranjang.Rows)
                {
                    if (row.IsNewRow)
                    {
                        continue;
                    }

                    string kodeBarang = row.Cells[0].Value?.ToString() ?? string.Empty;
                    if (kodeBarang.Length == 0)
                    {
                        continue;
                    }

                    int qty = InputHelper.AmbilInt(row.Cells[3].Value);
                    if (qty <= 0)
                    {
                        continue;
                    }

                    decimal subtotal = InputHelper.AmbilDecimal(row.Cells[4].Value);

                    // Kunci baris barang dan cek stok terbaru.
                    // FOR UPDATE mencegah dua kasir menjual stok yang sama.
                    string namaBarang;
                    int stokTerbaru;

                    using (MySqlCommand cmdCek = new(queryCekStok, conn, transaksi))
                    {
                        cmdCek.Parameters.AddWithValue("@kode", kodeBarang);
                        using MySqlDataReader reader = cmdCek.ExecuteReader();
                        if (!reader.Read())
                        {
                            throw new StokTidakCukupException(
                                "Barang \"" + kodeBarang + "\" tidak lagi ada di database.");
                        }

                        namaBarang = reader["nama_barang"]?.ToString() ?? kodeBarang;
                        stokTerbaru = InputHelper.AmbilInt(reader["stok"]);
                    }

                    if (stokTerbaru < qty)
                    {
                        throw new StokTidakCukupException(
                            "Stok \"" + namaBarang + "\" tidak cukup!\n"
                            + "Diminta: " + qty + ", tersedia: " + stokTerbaru + ".");
                    }

                    using (MySqlCommand cmdDetail = new(queryDetail, conn, transaksi))
                    {
                        cmdDetail.Parameters.AddWithValue("@noNota", noNota);
                        cmdDetail.Parameters.AddWithValue("@kode", kodeBarang);
                        cmdDetail.Parameters.AddWithValue("@qty", qty);
                        cmdDetail.Parameters.AddWithValue("@subtotal", subtotal);
                        cmdDetail.ExecuteNonQuery();
                    }

                    using (MySqlCommand cmdStok = new(queryStok, conn, transaksi))
                    {
                        cmdStok.Parameters.AddWithValue("@qty", qty);
                        cmdStok.Parameters.AddWithValue("@kode", kodeBarang);
                        cmdStok.ExecuteNonQuery();
                    }
                }

                transaksi.Commit();
                return noNota;
            }
            catch
            {
                // Batalkan semua perubahan bila ada satu yang gagal.
                try
                {
                    transaksi.Rollback();
                }
                catch
                {
                    // Rollback gagal (mis. koneksi putus) - tidak ada yang bisa dilakukan.
                }

                throw;
            }
        }

        private static string BuatNomorNota()
        {
            // Milidetik ditambahkan agar nomor nota tetap unik walaupun
            // ada dua transaksi pada detik yang sama.
            return "TRX-" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
        }

        private void ResetTransaksi()
        {
            dgvKeranjang.Rows.Clear();
            _totalBelanja = 0m;
            lblTotal.Text = "0";
            lblKembalian.Text = "0";
            txtBayar.Clear();
            txtBarcode.Clear();
            txtBarcode.Focus();
        }

        // =========================================================
        // 5. TAMPILAN
        // =========================================================
        private void dgvKeranjang_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            // KolomIndex = -1 terjadi saat event dipanggil untuk row header.
            if (e.ColumnIndex is not (2 or 4) || e.RowIndex < 0)
            {
                return;
            }

            object? nilai = dgvKeranjang.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
            if (nilai is null)
            {
                return;
            }

            e.Value = InputHelper.FormatNominal(InputHelper.AmbilDecimal(nilai));
            e.FormattingApplied = true;
        }

        private void FormKasir_FormClosing(object? sender, FormClosingEventArgs e)
        {
            // Bila kasir menutup form ini tanpa logout, aplikasi berhenti
            // agar tidak tertinggal proses berjalan tanpa jendela.
            if (!_sedangKeluar && IsRootForm)
            {
                _sedangKeluar = true;
                Session.Clear();
                Application.Exit();
            }
        }
    }

    /// <summary>Dilempar ketika stok tidak mencukupi saat menyimpan transaksi.</summary>
    internal sealed class StokTidakCukupException : Exception
    {
        public StokTidakCukupException(string message) : base(message)
        {
        }
    }
}
