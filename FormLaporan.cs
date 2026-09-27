using System.Data;
using System.Diagnostics;
using System.Text;
using Microsoft.Data.Sqlite;

namespace AplikasiKasirSMK4
{
    public partial class FormLaporan : Form
    {
        private readonly Koneksi _koneksi = new();

        // Tab tambahan untuk Laba Rugi dan Rekap Sales
        private readonly TabPage tabLaba = new();
        private readonly DataGridView dgvLaba = new();
        private readonly TabPage tabSales = new();
        private readonly DataGridView dgvSales = new();

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
            InisialisasiTabTambahan();

            UiThemeHelper.FormatTabel(dgvNota);
            UiThemeHelper.FormatTabel(dgvDetailNota);
            UiThemeHelper.FormatTabel(dgvDetail);
            UiThemeHelper.FormatTabel(dgvProduk);
            UiThemeHelper.FormatTabel(dgvLaba);
            UiThemeHelper.FormatTabel(dgvSales);
        }

        private void InisialisasiTabTambahan()
        {
            // 1. Tab Laba Rugi
            tabLaba.Text = "💵 Laba & Profit";
            tabLaba.Padding = new Padding(8);
            KonfigurasiGrid(dgvLaba);
            tabLaba.Controls.Add(dgvLaba);
            tabControl.TabPages.Add(tabLaba);
            dgvLaba.CellFormatting += dgvLaba_CellFormatting;

            // 2. Tab Rekap Sales
            tabSales.Text = "👔 Penjualan per Sales";
            tabSales.Padding = new Padding(8);
            KonfigurasiGrid(dgvSales);
            tabSales.Controls.Add(dgvSales);
            tabControl.TabPages.Add(tabSales);
            dgvSales.CellFormatting += dgvSales_CellFormatting;
        }

        private static void KonfigurasiGrid(DataGridView dgv)
        {
            UiThemeHelper.FormatTabel(dgv);
            dgv.Dock = DockStyle.Fill;
            dgv.ReadOnly = true;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void FormLaporan_Load(object sender, EventArgs e)
        {
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
                using SqliteConnection conn = _koneksi.GetConn();

                const string query = "SELECT id_user, nama_lengkap, username FROM tb_user ORDER BY nama_lengkap ASC";
                using SqliteCommand cmd = new(query, conn);
                using SqliteDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    int idUser = InputHelper.AmbilInt(reader["id_user"]);
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
                using SqliteConnection conn = _koneksi.GetConn();

                MuatRekapNota(conn, tglAwal, tglAkhir, idUserKasir);
                MuatDetailItem(conn, tglAwal, tglAkhir, idUserKasir);
                MuatProdukTerlaris(conn, tglAwal, tglAkhir, idUserKasir);
                MuatLaporanLaba(conn, tglAwal, tglAkhir, idUserKasir);
                MuatLaporanSales(conn, tglAwal, tglAkhir);
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
        private void MuatRekapNota(SqliteConnection conn, DateTime tglAwal, DateTime tglAkhir, int? idUserKasir)
        {
            string sql =
                "SELECT t.no_nota AS 'No. Nota', t.tanggal AS 'Waktu Transaksi', "
                + "u.nama_lengkap AS 'Kasir', "
                + "COALESCE(m.nama_metode, 'Tunai') AS 'Metode', "
                + "t.total_bayar AS 'Total Belanja', "
                + "t.uang_diterima AS 'Uang Diterima', t.kembalian AS 'Kembalian' "
                + "FROM tb_transaksi t "
                + "JOIN tb_user u ON t.id_user = u.id_user "
                + "LEFT JOIN tb_metode_bayar m ON m.id_metode = t.id_metode "
                + "WHERE t.tanggal BETWEEN @awal AND @akhir ";

            if (idUserKasir.HasValue)
            {
                sql += "AND t.id_user = @idUser ";
            }

            sql += "ORDER BY t.tanggal DESC";

            using SqliteCommand cmd = new(sql, conn);
            cmd.Parameters.AddWithValue("@awal", tglAwal);
            cmd.Parameters.AddWithValue("@akhir", tglAkhir);
            if (idUserKasir.HasValue)
            {
                cmd.Parameters.AddWithValue("@idUser", idUserKasir.Value);
            }

            DataTable dt = QueryHelper.IsiTabel(cmd);
            dgvNota.DataSource = dt;

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
                using SqliteConnection conn = _koneksi.GetConn();

                const string sql =
                    "SELECT kode_barcode AS 'Kode', nama_barang AS 'Nama Barang', "
                    + "qty AS 'Jumlah (Qty)', harga_satuan AS 'Harga Satuan', subtotal AS 'Subtotal' "
                    + "FROM tb_detail_transaksi WHERE no_nota = @noNota ORDER BY id_detail ASC";

                using SqliteCommand cmd = new(sql, conn);
                cmd.Parameters.AddWithValue("@noNota", noNota);

                DataTable dt = QueryHelper.IsiTabel(cmd);
                dgvDetailNota.DataSource = dt;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Gagal memuat detail nota: " + ex.Message);
            }
        }

        // ====================================================================
        // TAB 2: RINCIAN ITEM
        // ====================================================================
        private void MuatDetailItem(SqliteConnection conn, DateTime tglAwal, DateTime tglAkhir, int? idUserKasir)
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

            using SqliteCommand cmd = new(sql, conn);
            cmd.Parameters.AddWithValue("@awal", tglAwal);
            cmd.Parameters.AddWithValue("@akhir", tglAkhir);
            if (idUserKasir.HasValue)
            {
                cmd.Parameters.AddWithValue("@idUser", idUserKasir.Value);
            }

            DataTable dt = QueryHelper.IsiTabel(cmd);
            dgvDetail.DataSource = dt;

            decimal totalQty = 0m;
            foreach (DataRow row in dt.Rows)
            {
                totalQty += InputHelper.AmbilDecimal(row["Qty"]);
            }
            lblValQty.Text = InputHelper.FormatJumlah(totalQty) + " Item";
        }

        // ====================================================================
        // TAB 3: PRODUK TERLARIS
        // ====================================================================
        private void MuatProdukTerlaris(SqliteConnection conn, DateTime tglAwal, DateTime tglAkhir, int? idUserKasir)
        {
            string sql =
                "SELECT d.kode_barcode AS 'Kode Barcode', d.nama_barang AS 'Nama Barang', "
                + "SUM(d.qty) AS 'Total Terjual', ROUND(SUM(d.subtotal), 2) AS 'Total Penjualan' "
                + "FROM tb_transaksi t "
                + "JOIN tb_detail_transaksi d ON t.id_transaksi = d.id_transaksi "
                + "WHERE t.tanggal BETWEEN @awal AND @akhir ";

            if (idUserKasir.HasValue)
            {
                sql += "AND t.id_user = @idUser ";
            }

            sql += "GROUP BY d.kode_barcode, d.nama_barang ORDER BY SUM(d.qty) DESC";

            using SqliteCommand cmd = new(sql, conn);
            cmd.Parameters.AddWithValue("@awal", tglAwal);
            cmd.Parameters.AddWithValue("@akhir", tglAkhir);
            if (idUserKasir.HasValue)
            {
                cmd.Parameters.AddWithValue("@idUser", idUserKasir.Value);
            }

            DataTable dt = QueryHelper.IsiTabel(cmd);
            dgvProduk.DataSource = dt;
        }

        // ====================================================================
        // TAB 4: LAPORAN LABA RUGI (HPP & PROFIT)
        // ====================================================================
        private void MuatLaporanLaba(SqliteConnection conn, DateTime tglAwal, DateTime tglAkhir, int? idUserKasir)
        {
            string sql =
                "SELECT t.no_nota AS 'No. Nota', t.tanggal AS 'Waktu', "
                + "u.nama_lengkap AS 'Kasir', "
                + "ROUND(SUM(d.subtotal), 2) AS 'Penjualan (Omzet)', "
                + "ROUND(SUM(COALESCE(d.harga_beli_satuan, b.harga_beli, 0) * d.qty), 2) AS 'Modal (HPP)', "
                + "ROUND(SUM(d.subtotal) - SUM(COALESCE(d.harga_beli_satuan, b.harga_beli, 0) * d.qty), 2) AS 'Laba Kotor', "
                + "CASE WHEN SUM(d.subtotal) > 0 "
                + "     THEN ROUND(((SUM(d.subtotal) - SUM(COALESCE(d.harga_beli_satuan, b.harga_beli, 0) * d.qty)) * 100.0) / SUM(d.subtotal), 2) "
                + "     ELSE 0 END AS 'Margin (%)' "
                + "FROM tb_transaksi t "
                + "JOIN tb_user u ON t.id_user = u.id_user "
                + "JOIN tb_detail_transaksi d ON d.id_transaksi = t.id_transaksi "
                + "LEFT JOIN tb_barang b ON b.kode_barcode = d.kode_barcode "
                + "WHERE t.tanggal BETWEEN @awal AND @akhir ";

            if (idUserKasir.HasValue)
            {
                sql += "AND t.id_user = @idUser ";
            }

            sql += "GROUP BY t.id_transaksi, t.no_nota, t.tanggal, u.nama_lengkap ORDER BY t.tanggal DESC";

            using SqliteCommand cmd = new(sql, conn);
            cmd.Parameters.AddWithValue("@awal", tglAwal);
            cmd.Parameters.AddWithValue("@akhir", tglAkhir);
            if (idUserKasir.HasValue)
            {
                cmd.Parameters.AddWithValue("@idUser", idUserKasir.Value);
            }

            DataTable dt = QueryHelper.IsiTabel(cmd);
            dgvLaba.DataSource = dt;
        }

        // ====================================================================
        // TAB 5: REKAP PENJUALAN PER SALES
        // ====================================================================
        private void MuatLaporanSales(SqliteConnection conn, DateTime tglAwal, DateTime tglAkhir)
        {
            string sql =
                "SELECT COALESCE(s.nama_sales, '— Tanpa Sales —') AS 'Nama Sales', "
                + "COUNT(t.id_transaksi) AS 'Jumlah Nota', "
                + "ROUND(SUM(t.total_bayar), 2) AS 'Total Penjualan', "
                + "ROUND(COALESCE(SUM(k.jumlah_komisi), 0), 2) AS 'Total Komisi' "
                + "FROM tb_transaksi t "
                + "LEFT JOIN tb_sales s ON s.id_sales = t.id_sales "
                + "LEFT JOIN tb_komisi k ON k.id_transaksi = t.id_transaksi "
                + "WHERE t.tanggal BETWEEN @awal AND @akhir "
                + "GROUP BY t.id_sales, s.nama_sales ORDER BY SUM(t.total_bayar) DESC";

            using SqliteCommand cmd = new(sql, conn);
            cmd.Parameters.AddWithValue("@awal", tglAwal);
            cmd.Parameters.AddWithValue("@akhir", tglAkhir);

            DataTable dt = QueryHelper.IsiTabel(cmd);
            dgvSales.DataSource = dt;
        }

        // ====================================================================
        // FORMAT DATA GRID VIEW
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

        private void dgvLaba_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.Value is null || e.RowIndex < 0) return;

            string header = dgvLaba.Columns[e.ColumnIndex].HeaderText;
            if (header is "Penjualan (Omzet)" or "Modal (HPP)" or "Laba Kotor")
            {
                e.Value = "Rp " + InputHelper.FormatNominal(InputHelper.AmbilDecimal(e.Value));
                e.FormattingApplied = true;
            }
            else if (header == "Margin (%)")
            {
                e.Value = InputHelper.AmbilDecimal(e.Value).ToString("0.##") + "%";
                e.FormattingApplied = true;
            }
        }

        private void dgvSales_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.Value is null || e.RowIndex < 0) return;

            string header = dgvSales.Columns[e.ColumnIndex].HeaderText;
            if (header is "Total Penjualan" or "Total Komisi")
            {
                e.Value = "Rp " + InputHelper.FormatNominal(InputHelper.AmbilDecimal(e.Value));
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
            else if (tabControl.SelectedTab == tabProduk)
            {
                gridTarget = dgvProduk;
                namaFileDefault = $"Produk_Terlaris_{dtpMulai.Value:yyyyMMdd}_{dtpSelesai.Value:yyyyMMdd}.csv";
            }
            else if (tabControl.SelectedTab == tabLaba)
            {
                gridTarget = dgvLaba;
                namaFileDefault = $"Laporan_Laba_{dtpMulai.Value:yyyyMMdd}_{dtpSelesai.Value:yyyyMMdd}.csv";
            }
            else
            {
                gridTarget = dgvSales;
                namaFileDefault = $"Rekap_Sales_{dtpMulai.Value:yyyyMMdd}_{dtpSelesai.Value:yyyyMMdd}.csv";
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
            sfd.Title = "Simpan Laporan Sebagai CSV / Excel";

            if (sfd.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            try
            {
                StringBuilder sb = new();

                List<string> headers = new();
                foreach (DataGridViewColumn col in gridTarget.Columns)
                {
                    if (col.Visible)
                    {
                        headers.Add(EscapeCsv(col.HeaderText));
                    }
                }
                sb.AppendLine(string.Join(";", headers));

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
