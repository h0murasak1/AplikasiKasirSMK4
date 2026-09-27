using Microsoft.Data.Sqlite;

namespace AplikasiKasirSMK4
{
    /// <summary>
    /// Form untuk melihat dan membayar komisi sales.
    /// </summary>
    /// <remarks>
    /// Komisi dihitung dari persen yang ada di data sales dikalikan
    /// total nota yang dibawa sales tersebut. Setiap nota yang sudah
    /// dikaitkan dengan sales akan tercatat di tb_komisi.
    /// <para>
    /// Admin memilih baris komisi yang belum dibayar, lalu menandainya
    /// sebagai sudah dibayar. Tidak ada pembayaran sebagian per nota
    /// — komisi per nota dibayar sekaligus.
    /// </para>
    /// </remarks>
    public class FormKomisi : Form
    {
        // ─── Kontrol ────────────────────────────────────────────────
        private readonly DataGridView dgvKomisi = new();
        private readonly ComboBox cmbSales      = new();
        private readonly DateTimePicker dtpMulai  = new();
        private readonly DateTimePicker dtpSelesai = new();
        private readonly Button btnTampilkan = new();
        private readonly Button btnBayar     = new();
        private readonly Button btnRefresh   = new();
        private readonly CheckBox chkBelumBayar = new();
        private readonly Label lblRingkasan  = new();

        private readonly Koneksi _koneksi = new();

        // ─── Konstanta kolom ────────────────────────────────────────
        private const int KolId          = 0;
        private const int KolNoNota      = 1;
        private const int KolSales       = 2;
        private const int KolTanggal     = 3;
        private const int KolTotalNota   = 4;
        private const int KolKomisiPersen = 5;
        private const int KolJumlahKomisi = 6;
        private const int KolStatus      = 7;
        private const int KolTglBayar    = 8;

        public FormKomisi()
        {
            Text          = "Komisi Sales";
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize   = new Size(900, 520);
            ClientSize    = new Size(960, 560);
            Font          = new Font("Segoe UI", 9F);

            BangunTampilan();
            Load += FormKomisi_Load;
        }

        // ─── BANGUN TAMPILAN ─────────────────────────────────────────

        private void BangunTampilan()
        {
            var panelFilter = new Panel
            {
                Dock    = DockStyle.Top,
                Height  = 90,
                Padding = new Padding(8)
            };

            dtpMulai.Format   = DateTimePickerFormat.Short;
            dtpSelesai.Format = DateTimePickerFormat.Short;
            dtpMulai.Width    = 110;
            dtpSelesai.Width  = 110;
            cmbSales.Width    = 170;
            cmbSales.DropDownStyle = ComboBoxStyle.DropDownList;

            KonfigTombol(btnTampilkan, "🔍 Tampilkan",  Color.FromArgb(52, 152, 219));
            KonfigTombol(btnBayar,    "✓ Tandai Dibayar", Color.FromArgb(39, 174, 96));
            KonfigTombol(btnRefresh,  "↻ Muat",          Color.FromArgb(100, 100, 100));

            chkBelumBayar.Text     = "Hanya belum dibayar";
            chkBelumBayar.AutoSize = true;
            chkBelumBayar.Checked  = true;

            lblRingkasan.AutoSize  = true;
            lblRingkasan.Font      = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblRingkasan.ForeColor = Color.FromArgb(39, 174, 96);

            var baris1 = new FlowLayoutPanel
            {
                Location  = new Point(8, 8),
                Height    = 34,
                Width     = 800,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents  = false
            };
            baris1.Controls.AddRange(new Control[]
            {
                Lbl("Sales:"), cmbSales,
                Lbl("Dari:"), dtpMulai,
                Lbl("s/d:"), dtpSelesai,
                btnTampilkan,
                chkBelumBayar
            });

            var baris2 = new FlowLayoutPanel
            {
                Location  = new Point(8, 50),
                Height    = 34,
                Width     = 800,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents  = false
            };
            baris2.Controls.AddRange(new Control[] { btnBayar, btnRefresh, lblRingkasan });

            panelFilter.Controls.AddRange(new Control[] { baris1, baris2 });

            // DataGridView
            UiThemeHelper.FormatTabel(dgvKomisi);
            dgvKomisi.Dock                  = DockStyle.Fill;
            dgvKomisi.ReadOnly              = true;
            dgvKomisi.MultiSelect           = true;
            dgvKomisi.AllowUserToAddRows    = false;
            dgvKomisi.AllowUserToDeleteRows = false;
            dgvKomisi.AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill;

            dgvKomisi.Columns.Add(new DataGridViewTextBoxColumn { Name = "id_komisi",      HeaderText = "ID",       Visible = false });
            dgvKomisi.Columns.Add(new DataGridViewTextBoxColumn { Name = "no_nota",        HeaderText = "No. Nota", FillWeight = 17 });
            dgvKomisi.Columns.Add(new DataGridViewTextBoxColumn { Name = "nama_sales",     HeaderText = "Sales",    FillWeight = 18 });
            dgvKomisi.Columns.Add(new DataGridViewTextBoxColumn { Name = "tanggal",        HeaderText = "Tanggal",  FillWeight = 13 });
            dgvKomisi.Columns.Add(new DataGridViewTextBoxColumn { Name = "total_nota",     HeaderText = "Total Nota", FillWeight = 14,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight } });
            dgvKomisi.Columns.Add(new DataGridViewTextBoxColumn { Name = "komisi_persen",  HeaderText = "Komisi %", FillWeight = 10 });
            dgvKomisi.Columns.Add(new DataGridViewTextBoxColumn { Name = "jumlah_komisi",  HeaderText = "Komisi Rp", FillWeight = 14,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight } });
            dgvKomisi.Columns.Add(new DataGridViewTextBoxColumn { Name = "status",         HeaderText = "Status",   FillWeight = 10 });
            dgvKomisi.Columns.Add(new DataGridViewTextBoxColumn { Name = "tanggal_dibayar",HeaderText = "Tgl Bayar",FillWeight = 14 });

            Controls.Add(dgvKomisi);
            Controls.Add(panelFilter);

            btnTampilkan.Click += (_, _) => MuatData();
            btnBayar.Click     += BtnBayar_Click;
            btnRefresh.Click   += (_, _) => MuatData();
            dgvKomisi.SelectionChanged += (_, _) =>
                btnBayar.Enabled = dgvKomisi.SelectedRows.Count > 0;
        }

        private static Label Lbl(string t) => new()
        {
            Text = t, TextAlign = ContentAlignment.MiddleLeft,
            AutoSize = true, Margin = new Padding(4, 4, 2, 0)
        };

        private static void KonfigTombol(Button btn, string teks, Color warna)
        {
            btn.Text = teks; btn.Width = 140; btn.Height = 28;
            btn.FlatStyle = FlatStyle.Flat; btn.BackColor = warna; btn.ForeColor = Color.White;
            btn.Cursor = Cursors.Hand; btn.FlatAppearance.BorderSize = 0;
            btn.Margin = new Padding(0, 3, 6, 0);
            btn.FlatAppearance.MouseOverBackColor = ControlPaint.Light(warna, 0.15f);
        }

        // ─── LOAD ────────────────────────────────────────────────────

        private void FormKomisi_Load(object? sender, EventArgs e)
        {
            if (!Session.IsAdmin)
            {
                MessageBox.Show("Hanya Admin yang dapat mengakses halaman ini.",
                    "Akses Ditolak", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }

            DateTime hari = DateTime.Today;
            dtpMulai.Value  = new DateTime(hari.Year, hari.Month, 1);
            dtpSelesai.Value = hari;

            MuatListSales();
            MuatData();
        }

        private void MuatListSales()
        {
            cmbSales.Items.Clear();
            cmbSales.Items.Add(new ComboItem(null, "— Semua Sales —"));

            try
            {
                using SqliteConnection conn = _koneksi.GetConn();
                using SqliteCommand cmd = new(
                    "SELECT id_sales, nama_sales FROM tb_sales WHERE is_active = 1 ORDER BY nama_sales", conn);
                using SqliteDataReader r = cmd.ExecuteReader();
                while (r.Read())
                {
                    int    id   = InputHelper.AmbilInt(r["id_sales"]);
                    string nama = r["nama_sales"]?.ToString() ?? "";
                    cmbSales.Items.Add(new ComboItem(id, nama));
                }
            }
            catch { /* biarkan list kosong */ }

            cmbSales.SelectedIndex = 0;
        }

        private void MuatData()
        {
            dgvKomisi.Rows.Clear();

            string mulai   = dtpMulai.Value.ToString("yyyy-MM-dd");
            string selesai = dtpSelesai.Value.ToString("yyyy-MM-dd");

            string query =
                "SELECT k.id_komisi, k.no_nota, s.nama_sales, k.tanggal, "
              + "    k.nilai_nota, k.persen, k.jumlah_komisi, "
              + "    k.sudah_dibayar, k.tanggal_dibayar "
              + "FROM tb_komisi k "
              + "JOIN tb_sales s ON s.id_sales = k.id_sales "
              + "WHERE DATE(k.tanggal) BETWEEN @mulai AND @selesai ";

            if (chkBelumBayar.Checked)
                query += " AND k.sudah_dibayar = 0 ";

            if (cmbSales.SelectedItem is ComboItem item && item.Id.HasValue)
                query += $" AND k.id_sales = {item.Id.Value} ";

            query += " ORDER BY k.tanggal DESC";

            decimal totalKomisi = 0m;
            decimal belumDibayar = 0m;

            try
            {
                using SqliteConnection conn = _koneksi.GetConn();
                using SqliteCommand cmd = new(query, conn);
                cmd.Parameters.AddWithValue("@mulai",   mulai);
                cmd.Parameters.AddWithValue("@selesai", selesai);

                using SqliteDataReader r = cmd.ExecuteReader();
                while (r.Read())
                {
                    int     id      = InputHelper.AmbilInt(r["id_komisi"]);
                    string  nota    = r["no_nota"]?.ToString() ?? "";
                    string  sales   = r["nama_sales"]?.ToString() ?? "";
                    string  tgl     = r["tanggal"]?.ToString() ?? "";
                    decimal total   = InputHelper.AmbilDecimal(r["nilai_nota"]);
                    decimal persen  = InputHelper.AmbilDecimal(r["persen"]);
                    decimal komisi  = InputHelper.AmbilDecimal(r["jumlah_komisi"]);
                    bool    dibayar = InputHelper.AmbilDecimal(r["sudah_dibayar"]) == 1m;
                    string  tglByr  = r["tanggal_dibayar"]?.ToString() ?? "-";

                    dgvKomisi.Rows.Add(
                        id, nota, sales, tgl,
                        InputHelper.FormatNominal(total),
                        persen.ToString("0.##") + "%",
                        InputHelper.FormatNominal(komisi),
                        dibayar ? "DIBAYAR" : "BELUM",
                        tglByr);

                    if (dibayar)
                    {
                        int i = dgvKomisi.Rows.Count - 1;
                        dgvKomisi.Rows[i].DefaultCellStyle.ForeColor = Color.Gray;
                    }

                    totalKomisi += komisi;
                    if (!dibayar) belumDibayar += komisi;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat komisi:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            lblRingkasan.Text =
                $"Total: {InputHelper.FormatNominal(totalKomisi)}  |  "
              + $"Belum dibayar: {InputHelper.FormatNominal(belumDibayar)}";

            btnBayar.Enabled = false;
        }

        // ─── TANDAI DIBAYAR ──────────────────────────────────────────

        private void BtnBayar_Click(object? sender, EventArgs e)
        {
            if (dgvKomisi.SelectedRows.Count == 0)
            {
                MessageBox.Show("Pilih minimal satu baris komisi.",
                    "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Kumpulkan id komisi yang belum dibayar
            var idList = new List<int>();
            decimal totalYangDibayar = 0m;

            foreach (DataGridViewRow baris in dgvKomisi.SelectedRows)
            {
                string status = baris.Cells[KolStatus].Value?.ToString() ?? "";
                if (status == "DIBAYAR") continue;
                idList.Add(InputHelper.AmbilInt(baris.Cells[KolId].Value));
                totalYangDibayar += InputHelper.AmbilDecimal(
                    baris.Cells[KolJumlahKomisi].Value?.ToString()?.Replace(".", "").Replace(",", ".") ?? "0");
            }

            if (idList.Count == 0)
            {
                MessageBox.Show("Semua baris yang dipilih sudah dibayar.",
                    "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var ok = MessageBox.Show(
                $"Tandai {idList.Count} komisi sebagai SUDAH DIBAYAR?\n\nTotal: {InputHelper.FormatNominal(totalYangDibayar)}",
                "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (ok != DialogResult.Yes) return;

            try
            {
                using SqliteConnection conn = _koneksi.GetConn();
                string tglBayar = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                foreach (int id in idList)
                {
                    using SqliteCommand cmd = new(
                        "UPDATE tb_komisi SET sudah_dibayar = 1, tanggal_dibayar = @tgl, id_user = @user "
                        + "WHERE id_komisi = @id", conn);
                    cmd.Parameters.AddWithValue("@tgl",  tglBayar);
                    cmd.Parameters.AddWithValue("@user", Session.UserId);
                    cmd.Parameters.AddWithValue("@id",   id);
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show($"{idList.Count} komisi berhasil ditandai dibayar.", "Berhasil",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                MuatData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal menyimpan:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ─── ComboItem helper ─────────────────────────────────────────

        private sealed class ComboItem
        {
            public int? Id { get; }
            private readonly string _teks;

            public ComboItem(int? id, string teks) { Id = id; _teks = teks; }
            public override string ToString() => _teks;
        }
    }
}
