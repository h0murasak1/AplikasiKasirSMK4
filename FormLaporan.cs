using System.Data;
using System.Diagnostics;
using System.Text;
using MySql.Data.MySqlClient;

namespace AplikasiKasirSMK4
{
    public partial class FormLaporan : Form
    {
        private readonly Koneksi _koneksi = new();

        private sealed class KasirItem
        {
            public int? IdUser { get; }
            public string Teks { get; }

            public KasirItem(int? idUser, string teks)
            {
                IdUser = idUser;
                Teks = teks;
            }

            public override string ToString() => Teks;
        }

        public FormLaporan()
        {
            InitializeComponent();
        }

        private void FormLaporan_Load(object sender, EventArgs e)
        {
            // Guard role: Laporan penjualan hanya dapat dibuka oleh Admin.
            if (!Session.IsAdmin)
            {
                MessageBox.Show(
                    "Hanya akun Admin yang dapat mengakses laporan penjualan.",
                    "Akses Ditolak",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                Close();
                return;
            }

            // Inisialisasi filter tanggal: awal bulan ini s/d hari ini
            DateTime hariIni = DateTime.Today;
            dtpMulai.Value = new DateTime(hariIni.Year, hariIni.Month, 1);
            dtpSelesai.Value = hariIni;

            MuatDaftarKasir();
            MuatLaporan();
        }

        private void MuatDaftarKasir()
        {
            cmbKasir.Items.Clear();
            cmbKasir.Items.Add(new KasirItem(null, "Semua Kasir"));

            try
            {
                using MySqlConnection conn = _koneksi.GetConn();
                conn.Open();

                const string query = "SELECT id_user, nama_lengkap, username FROM tb_user ORDER BY nama_lengkap ASC";
                using MySqlCommand cmd = new(query, conn);
                using MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    int idUser = reader.GetInt32("id_user");
                    string nama = reader["nama_lengkap"]?.ToString() ?? "User";
                    string username = reader["username"]?.ToString() ?? string.Empty;
                    cmbKasir.Items.Add(new KasirItem(idUser, $"{nama} ({username})"));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat daftar kasir: " + ex.Message, "Peringatan",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            cmbKasir.SelectedIndex = 0;
        }

        private void btnHariIni_Click(object sender, EventArgs e)
        {
            dtpMulai.Value = DateTime.Today;
            dtpSelesai.Value = DateTime.Today;
            MuatLaporan();
        }

        private void btnBulanIni_Click(object sender, EventArgs e)
        {
            DateTime hariIni = DateTime.Today;
            dtpMulai.Value = new DateTime(hariIni.Year, hariIni.Month, 1);
            dtpSelesai.Value = hariIni;
            MuatLaporan();
        }

        private void btnFilter_Click(object sender, EventArgs e)
        {
            MuatLaporan();
        }

        private void MuatLaporan()
        {
            DateTime tglAwal = dtpMulai.Value.Date;
            DateTime tglAkhir = dtpSelesai.Value.Date.AddDays(1).AddSeconds(-1);

            if (tglAwal > tglAkhir)
            {
                MessageBox.Show("Tanggal awal tidak boleh melebihi tanggal akhir!", "Peringatan",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int? idUserKasir = (cmbKasir.SelectedItem as KasirItem)?.IdUser;

            try
            {
                using MySqlConnection conn = _koneksi.GetConn();
                conn.Open();

                MuatRekapNota(conn, tglAwal, tglAkhir, idUserKasir);
                MuatDetailItem(conn, tglAwal, tglAkhir, idUserKasir);
                MuatProdukTerlaris(conn, tglAwal, tglAkhir, idUserKasir);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat laporan: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ====================================================================
        // TAB 1: REKAP NOTA
        // ====================================================================
        private void MuatRekapNota(MySqlConnection conn, DateTime tglAwal, DateTime tglAkhir, int? idUserKasir)
        {
            string sql =
                "SELECT t.no_nota AS 'No. Nota', t.tanggal AS 'Waktu Transaksi', "
                + "u.nama_lengkap AS 'Kasir', t.total_bayar AS 'Total Belanja', "
                + "t.uang_diterima AS 'Uang Diterima', t.kembalian AS 'Kembalian' "
                + "FROM tb_transaksi t "
                + "JOIN tb_user u ON t.id_user = u.id_user "
                + "WHERE t.tanggal BETWEEN @awal AND @akhir ";

            if (idUserKasir.HasValue)
            {
                sql += "AND t.id_user = @idUser ";
            }

            sql += "ORDER BY t.tanggal DESC";

            using MySqlCommand cmd = new(sql, conn);
            cmd.Parameters.AddWithValue("@awal", tglAwal);
            cmd.Parameters.AddWithValue("@akhir", tglAkhir);
            if (idUserKasir.HasValue)
            {
                cmd.Parameters.AddWithValue("@idUser", idUserKasir.Value);
            }

            using MySqlDataAdapter adapter = new(cmd);
            DataTable dt = new();
            adapter.Fill(dt);
            dgvNota.DataSource = dt;

            // Hitung KPI
            decimal totalOmzet = 0m;
            int totalTransaksi = dt.Rows.Count;

            foreach (DataRow row in dt.Rows)
            {
                totalOmzet += InputHelper.AmbilDecimal(row["Total Belanja"]);
            }

            lblValOmzet.Text = "Rp " + InputHelper.FormatNominal(totalOmzet);
            lblValTransaksi.Text = totalTransaksi.ToString("N0") + " Nota";

            if (dt.Rows.Count == 0)
            {
                dgvDetailNota.DataSource = null;
            }
        }

        private void dgvNota_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvNota.CurrentRow is null || dgvNota.CurrentRow.DataBoundItem is not DataRowView rowView)
            {
                dgvDetailNota.DataSource = null;
                return;
            }

            string noNota = Convert.ToString(rowView["No. Nota"]) ?? string.Empty;
            if (string.IsNullOrEmpty(noNota))
            {
                dgvDetailNota.DataSource = null;
                return;
            }

            MuatDetailPerNota(noNota);
        }

        private void MuatDetailPerNota(string noNota)
        {
            try
            {
                using MySqlConnection conn = _koneksi.GetConn();
                conn.Open();

                const string sql =
                    "SELECT kode_barcode AS 'Kode', nama_barang AS 'Nama Barang', "
                    + "qty AS 'Jumlah (Qty)', harga_satuan AS 'Harga Satuan', subtotal AS 'Subtotal' "
                    + "FROM tb_detail_transaksi WHERE no_nota = @noNota ORDER BY id_detail ASC";

                using MySqlCommand cmd = new(sql, conn);
                cmd.Parameters.AddWithValue("@noNota", noNota);

                using MySqlDataAdapter adapter = new(cmd);
                DataTable dt = new();
                adapter.Fill(dt);
                dgvDetailNota.DataSource = dt;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Gagal memuat detail nota: " + ex.Message);
            }
        }

        // ====================================================================
        // TAB 2: RINCIAN ITEM (DARI VIEW v_laporan_penjualan)
        // ====================================================================
        private void MuatDetailItem(MySqlConnection conn, DateTime tglAwal, DateTime tglAkhir, int? idUserKasir)
        {
            string sql =
                "SELECT no_nota AS 'No. Nota', tanggal AS 'Waktu', nama_lengkap AS 'Kasir', "
                + "kode_barcode AS 'Kode', nama_barang AS 'Nama Barang', qty AS 'Qty', "
                + "harga_satuan AS 'Harga', subtotal AS 'Subtotal' "
                + "FROM v_laporan_penjualan "
                + "WHERE tanggal BETWEEN @awal AND @akhir ";

            if (idUserKasir.HasValue)
            {
                sql += "AND username = (SELECT username FROM tb_user WHERE id_user = @idUser LIMIT 1) ";
            }

            sql += "ORDER BY tanggal DESC";

            using MySqlCommand cmd = new(sql, conn);
            cmd.Parameters.AddWithValue("@awal", tglAwal);
            cmd.Parameters.AddWithValue("@akhir", tglAkhir);
            if (idUserKasir.HasValue)
            {
                cmd.Parameters.AddWithValue("@idUser", idUserKasir.Value);
            }

            using MySqlDataAdapter adapter = new(cmd);
            DataTable dt = new();
            adapter.Fill(dt);
            dgvDetail.DataSource = dt;

            // Hitung total item terjual
            decimal totalQty = 0m;
            foreach (DataRow row in dt.Rows)
            {
                totalQty += InputHelper.AmbilDecimal(row["Qty"]);
            }
            lblValQty.Text = InputHelper.FormatJumlah(totalQty) + " Item";
        }

        // ====================================================================
        // TAB 3: PRODUK TERLARIS (AGGREGATE)
        // ====================================================================
        private void MuatProdukTerlaris(MySqlConnection conn, DateTime tglAwal, DateTime tglAkhir, int? idUserKasir)
        {
            string sql =
                "SELECT d.kode_barcode AS 'Kode Barcode', d.nama_barang AS 'Nama Barang', "
                + "SUM(d.qty) AS 'Total Terjual', SUM(d.subtotal) AS 'Total Penjualan' "
                + "FROM tb_transaksi t "
                + "JOIN tb_detail_transaksi d ON t.id_transaksi = d.id_transaksi "
                + "WHERE t.tanggal BETWEEN @awal AND @akhir ";

            if (idUserKasir.HasValue)
            {
                sql += "AND t.id_user = @idUser ";
            }

            sql += "GROUP BY d.kode_barcode, d.nama_barang ORDER BY SUM(d.qty) DESC";

            using MySqlCommand cmd = new(sql, conn);
            cmd.Parameters.AddWithValue("@awal", tglAwal);
            cmd.Parameters.AddWithValue("@akhir", tglAkhir);
            if (idUserKasir.HasValue)
            {
                cmd.Parameters.AddWithValue("@idUser", idUserKasir.Value);
            }

            using MySqlDataAdapter adapter = new(cmd);
            DataTable dt = new();
            adapter.Fill(dt);
            dgvProduk.DataSource = dt;
        }

        // ====================================================================
        // FORMAT DATA GRID VIEW (RUPIAH & PECAHAN)
        // ====================================================================
        private void dgvNota_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.Value is null || e.RowIndex < 0) return;

            string header = dgvNota.Columns[e.ColumnIndex].HeaderText;
            if (header is "Total Belanja" or "Uang Diterima" or "Kembalian")
            {
                e.Value = "Rp " + InputHelper.FormatNominal(InputHelper.AmbilDecimal(e.Value));
                e.FormattingApplied = true;
            }
            else if (header == "Waktu Transaksi" && e.Value is DateTime dt)
            {
                e.Value = dt.ToString("dd/MM/yyyy HH:mm:ss");
                e.FormattingApplied = true;
            }
        }

        private void dgvDetailNota_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.Value is null || e.RowIndex < 0) return;

            string header = dgvDetailNota.Columns[e.ColumnIndex].HeaderText;
            if (header is "Harga Satuan" or "Subtotal")
            {
                e.Value = "Rp " + InputHelper.FormatNominal(InputHelper.AmbilDecimal(e.Value));
                e.FormattingApplied = true;
            }
            else if (header == "Jumlah (Qty)")
            {
                e.Value = InputHelper.FormatJumlah(InputHelper.AmbilDecimal(e.Value));
                e.FormattingApplied = true;
            }
        }

        private void dgvDetail_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.Value is null || e.RowIndex < 0) return;

            string header = dgvDetail.Columns[e.ColumnIndex].HeaderText;
            if (header is "Harga" or "Subtotal")
            {
                e.Value = "Rp " + InputHelper.FormatNominal(InputHelper.AmbilDecimal(e.Value));
                e.FormattingApplied = true;
            }
            else if (header == "Qty")
            {
                e.Value = InputHelper.FormatJumlah(InputHelper.AmbilDecimal(e.Value));
                e.FormattingApplied = true;
            }
            else if (header == "Waktu" && e.Value is DateTime dt)
            {
                e.Value = dt.ToString("dd/MM/yyyy HH:mm");
                e.FormattingApplied = true;
            }
        }

        private void dgvProduk_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.Value is null || e.RowIndex < 0) return;

            string header = dgvProduk.Columns[e.ColumnIndex].HeaderText;
            if (header == "Total Penjualan")
            {
                e.Value = "Rp " + InputHelper.FormatNominal(InputHelper.AmbilDecimal(e.Value));
                e.FormattingApplied = true;
            }
            else if (header == "Total Terjual")
            {
                e.Value = InputHelper.FormatJumlah(InputHelper.AmbilDecimal(e.Value));
                e.FormattingApplied = true;
            }
        }

        // ====================================================================
        // EKSPOR CSV
        // ====================================================================
        private void btnEkspor_Click(object sender, EventArgs e)
        {
            DataGridView gridTarget;
            string namaFileDefault;

            if (tabControl.SelectedTab == tabNota)
            {
                gridTarget = dgvNota;
                namaFileDefault = $"Rekap_Nota_{dtpMulai.Value:yyyyMMdd}_{dtpSelesai.Value:yyyyMMdd}.csv";
            }
            else if (tabControl.SelectedTab == tabDetail)
            {
                gridTarget = dgvDetail;
                namaFileDefault = $"Rincian_Penjualan_{dtpMulai.Value:yyyyMMdd}_{dtpSelesai.Value:yyyyMMdd}.csv";
            }
            else
            {
                gridTarget = dgvProduk;
                namaFileDefault = $"Produk_Terlaris_{dtpMulai.Value:yyyyMMdd}_{dtpSelesai.Value:yyyyMMdd}.csv";
            }

            if (gridTarget.Rows.Count == 0)
            {
                MessageBox.Show("Tidak ada data untuk diekspor!", "Informasi",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using SaveFileDialog sfd = new();
            sfd.Filter = "CSV File (*.csv)|*.csv|All Files (*.*)|*.*";
            sfd.FileName = namaFileDefault;
            sfd.Title = "Simpan Laporan Penjualan Sebagai CSV / Excel";

            if (sfd.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            try
            {
                StringBuilder sb = new();

                // Header kolom
                List<string> headers = new();
                foreach (DataGridViewColumn col in gridTarget.Columns)
                {
                    if (col.Visible)
                    {
                        headers.Add(EscapeCsv(col.HeaderText));
                    }
                }
                sb.AppendLine(string.Join(";", headers));

                // Baris data
                foreach (DataGridViewRow row in gridTarget.Rows)
                {
                    if (row.IsNewRow) continue;

                    List<string> values = new();
                    foreach (DataGridViewColumn col in gridTarget.Columns)
                    {
                        if (col.Visible)
                        {
                            object? val = row.Cells[col.Index].Value;
                            string valStr;
                            if (val is DateTime d)
                            {
                                valStr = d.ToString("yyyy-MM-dd HH:mm:ss");
                            }
                            else if (val is decimal dec)
                            {
                                valStr = dec.ToString("0.##");
                            }
                            else
                            {
                                valStr = val?.ToString() ?? string.Empty;
                            }
                            values.Add(EscapeCsv(valStr));
                        }
                    }
                    sb.AppendLine(string.Join(";", values));
                }

                // Tulis dengan UTF-8 with BOM agar terbaca sempurna di Microsoft Excel
                File.WriteAllText(sfd.FileName, sb.ToString(), new UTF8Encoding(true));

                DialogResult buka = MessageBox.Show(
                    "Laporan berhasil diekspor ke:\n" + sfd.FileName + "\n\nBuka berkas sekarang?",
                    "Ekspor Sukses",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);

                if (buka == DialogResult.Yes)
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = sfd.FileName,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal mengekspor data: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string EscapeCsv(string nilai)
        {
            if (nilai.Contains(';') || nilai.Contains('"') || nilai.Contains('\n') || nilai.Contains('\r'))
            {
                return "\"" + nilai.Replace("\"", "\"\"") + "\"";
            }
            return nilai;
        }
    }
}
