using Microsoft.Data.Sqlite;

namespace AplikasiKasirSMK4
{
    /// <summary>
    /// Form untuk melihat dan mengelola data piutang member.
    /// </summary>
    /// <remarks>
    /// Piutang timbul ketika transaksi kasir menggunakan metode bayar
    /// TEMPO. Form ini menampilkan semua piutang (yang belum lunas
    /// maupun yang sudah), dan memungkinkan admin mencatat pembayaran
    /// sebagian atau lunas.
    /// <para>
    /// Sisa piutang = jumlah_piutang - sudah_bayar. Status:
    /// LUNAS bila sisa = 0, SEBAGIAN bila 0 &lt; sisa &lt; total,
    /// BELUM bila sisa = total.
    /// </para>
    /// </remarks>
    public class FormPiutang : Form
    {
        // ─── Kontrol ────────────────────────────────────────────────
        private readonly DataGridView dgvPiutang = new();
        private readonly Button btnBayar   = new();
        private readonly Button btnRefresh = new();
        private readonly ComboBox cmbFilter = new();
        private readonly DateTimePicker dtpMulai  = new();
        private readonly DateTimePicker dtpSelesai = new();
        private readonly Button btnTampilkan = new();
        private readonly Label lblRingkasan  = new();

        private readonly Koneksi _koneksi = new();

        // ─── Konstanta kolom ────────────────────────────────────────
        private const int KolId         = 0;
        private const int KolNoNota     = 1;
        private const int KolMember     = 2;
        private const int KolTanggal    = 3;
        private const int KolJatuhTempo = 4;
        private const int KolJumlah     = 5;
        private const int KolSudahBayar = 6;
        private const int KolSisa       = 7;
        private const int KolStatus     = 8;

        public FormPiutang()
        {
            Text          = "Data Piutang";
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize   = new Size(900, 520);
            ClientSize    = new Size(960, 560);
            Font          = new Font("Segoe UI", 9F);

            BangunTampilan();
            Load += FormPiutang_Load;
        }

        // ─── BANGUN TAMPILAN ─────────────────────────────────────────

        private void BangunTampilan()
        {
            // Panel filter
            var panelFilter = new Panel
            {
                Dock   = DockStyle.Top,
                Height = 90,
                Padding = new Padding(8)
            };

            dtpMulai.Format  = DateTimePickerFormat.Short;
            dtpSelesai.Format = DateTimePickerFormat.Short;
            dtpMulai.Width   = 110;
            dtpSelesai.Width = 110;

            cmbFilter.Width     = 140;
            cmbFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbFilter.Items.AddRange(new object[] { "Semua", "Belum Lunas", "Sebagian", "Lunas" });
            cmbFilter.SelectedIndex = 1; // default Belum Lunas

            KonfigTombol(btnTampilkan, "🔍 Tampilkan", Color.FromArgb(52, 152, 219));
            KonfigTombol(btnBayar,     "💰 Catat Bayar", Color.FromArgb(39, 174, 96));
            KonfigTombol(btnRefresh,   "↻ Muat",         Color.FromArgb(100, 100, 100));

            lblRingkasan.AutoSize  = true;
            lblRingkasan.Font      = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblRingkasan.ForeColor = Color.FromArgb(231, 76, 60);

            // Baris 1
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
                Lbl("Dari:"), dtpMulai,
                Lbl("s/d:"), dtpSelesai,
                Lbl("Status:"), cmbFilter,
                btnTampilkan
            });

            // Baris 2
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
            UiThemeHelper.FormatTabel(dgvPiutang);
            dgvPiutang.Dock                  = DockStyle.Fill;
            dgvPiutang.ReadOnly              = true;
            dgvPiutang.AllowUserToAddRows    = false;
            dgvPiutang.AllowUserToDeleteRows = false;
            dgvPiutang.AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill;

            dgvPiutang.Columns.Add(new DataGridViewTextBoxColumn { Name = "id_piutang",   HeaderText = "ID",         Visible = false });
            dgvPiutang.Columns.Add(new DataGridViewTextBoxColumn { Name = "no_nota",       HeaderText = "No. Nota",   FillWeight = 16 });
            dgvPiutang.Columns.Add(new DataGridViewTextBoxColumn { Name = "nama_member",   HeaderText = "Member",     FillWeight = 20 });
            dgvPiutang.Columns.Add(new DataGridViewTextBoxColumn { Name = "tanggal",       HeaderText = "Tanggal",    FillWeight = 14 });
            dgvPiutang.Columns.Add(new DataGridViewTextBoxColumn { Name = "jatuh_tempo",   HeaderText = "Jatuh Tempo",FillWeight = 14 });
            dgvPiutang.Columns.Add(new DataGridViewTextBoxColumn { Name = "jumlah",        HeaderText = "Jumlah",     FillWeight = 13, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight } });
            dgvPiutang.Columns.Add(new DataGridViewTextBoxColumn { Name = "sudah_bayar",   HeaderText = "Sudah Bayar",FillWeight = 13, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight } });
            dgvPiutang.Columns.Add(new DataGridViewTextBoxColumn { Name = "sisa",          HeaderText = "Sisa",       FillWeight = 13, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight } });
            dgvPiutang.Columns.Add(new DataGridViewTextBoxColumn { Name = "status",        HeaderText = "Status",     FillWeight = 10 });

            Controls.Add(dgvPiutang);
            Controls.Add(panelFilter);

            btnTampilkan.Click += (_, _) => MuatData();
            btnBayar.Click     += BtnBayar_Click;
            btnRefresh.Click   += (_, _) => MuatData();
            dgvPiutang.SelectionChanged += (_, _) => btnBayar.Enabled = dgvPiutang.SelectedRows.Count > 0;
            dgvPiutang.CellFormatting   += DgvPiutang_CellFormatting;
        }

        private static Label Lbl(string teks) => new()
        {
            Text      = teks,
            TextAlign = ContentAlignment.MiddleLeft,
            AutoSize  = true,
            Margin    = new Padding(4, 4, 2, 0)
        };

        private static void KonfigTombol(Button btn, string teks, Color warna)
        {
            btn.Text      = teks;
            btn.Width     = 120;
            btn.Height    = 28;
            btn.FlatStyle = FlatStyle.Flat;
            btn.BackColor = warna;
            btn.ForeColor = Color.White;
            btn.Cursor    = Cursors.Hand;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = ControlPaint.Light(warna, 0.15f);
            btn.Margin    = new Padding(0, 3, 6, 0);
        }

        // ─── LOAD DATA ───────────────────────────────────────────────

        private void FormPiutang_Load(object? sender, EventArgs e)
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
            MuatData();
        }

        private void MuatData()
        {
            dgvPiutang.Rows.Clear();

            string mulai   = dtpMulai.Value.ToString("yyyy-MM-dd");
            string selesai = dtpSelesai.Value.ToString("yyyy-MM-dd");
            string filter  = cmbFilter.SelectedItem?.ToString() ?? "Semua";

            string query =
                "SELECT p.id_piutang, p.no_nota, m.nama_member, p.tanggal, p.jatuh_tempo, "
              + "    p.jumlah_piutang, p.sudah_bayar, "
              + "    ROUND(p.jumlah_piutang - p.sudah_bayar, 2) AS sisa "
              + "FROM tb_piutang p "
              + "JOIN tb_member m ON m.id_member = p.id_member "
              + "WHERE DATE(p.tanggal) BETWEEN @mulai AND @selesai ";

            query += filter switch
            {
                "Belum Lunas" => " AND p.sudah_bayar = 0 ",
                "Sebagian"    => " AND p.sudah_bayar > 0 AND p.sudah_bayar < p.jumlah_piutang ",
                "Lunas"       => " AND p.sudah_bayar >= p.jumlah_piutang ",
                _             => ""
            };
            query += " ORDER BY p.tanggal DESC";

            decimal totalPiutang = 0m;
            decimal totalSisa    = 0m;

            try
            {
                using SqliteConnection conn = _koneksi.GetConn();
                using SqliteCommand cmd = new(query, conn);
                cmd.Parameters.AddWithValue("@mulai",   mulai);
                cmd.Parameters.AddWithValue("@selesai", selesai);

                using SqliteDataReader r = cmd.ExecuteReader();
                while (r.Read())
                {
                    int     id     = InputHelper.AmbilInt(r["id_piutang"]);
                    string  nota   = r["no_nota"]?.ToString() ?? "";
                    string  member = r["nama_member"]?.ToString() ?? "";
                    string  tgl    = r["tanggal"]?.ToString() ?? "";
                    string  tempo  = r["jatuh_tempo"]?.ToString() ?? "-";
                    decimal jumlah = InputHelper.AmbilDecimal(r["jumlah_piutang"]);
                    decimal bayar  = InputHelper.AmbilDecimal(r["sudah_bayar"]);
                    decimal sisa   = InputHelper.AmbilDecimal(r["sisa"]);

                    string status = sisa <= 0 ? "LUNAS"
                        : bayar > 0           ? "SEBAGIAN"
                        :                       "BELUM";

                    int baris = dgvPiutang.Rows.Add(
                        id, nota, member, tgl, tempo,
                        InputHelper.FormatNominal(jumlah),
                        InputHelper.FormatNominal(bayar),
                        InputHelper.FormatNominal(sisa),
                        status);

                    // Warnai baris sesuai status
                    Color? warnaBaris = status switch
                    {
                        "LUNAS"    => Color.FromArgb(230, 255, 230),
                        "SEBAGIAN" => Color.FromArgb(255, 252, 220),
                        _          => (Color?)null
                    };
                    if (warnaBaris.HasValue)
                        dgvPiutang.Rows[baris].DefaultCellStyle.BackColor = warnaBaris.Value;

                    // Merah jika jatuh tempo sudah lewat dan belum lunas
                    if (status != "LUNAS" && tempo != "-"
                        && DateTime.TryParse(tempo, out DateTime tglTempo)
                        && tglTempo < DateTime.Today)
                    {
                        dgvPiutang.Rows[baris].DefaultCellStyle.ForeColor = Color.FromArgb(192, 0, 0);
                    }

                    totalPiutang += jumlah;
                    totalSisa    += sisa;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat data piutang:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            lblRingkasan.Text = $"Total Piutang: {InputHelper.FormatNominal(totalPiutang)}  |  "
                              + $"Sisa: {InputHelper.FormatNominal(totalSisa)}";

            btnBayar.Enabled = false;
        }

        // ─── CATAT PEMBAYARAN ─────────────────────────────────────────

        private void BtnBayar_Click(object? sender, EventArgs e)
        {
            if (dgvPiutang.SelectedRows.Count == 0)
            {
                MessageBox.Show("Pilih satu piutang terlebih dahulu.",
                    "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int    id    = InputHelper.AmbilInt(dgvPiutang.SelectedRows[0].Cells[KolId].Value);
            string nota  = dgvPiutang.SelectedRows[0].Cells[KolNoNota].Value?.ToString() ?? "";
            string sisa  = dgvPiutang.SelectedRows[0].Cells[KolSisa].Value?.ToString() ?? "0";

            string status = dgvPiutang.SelectedRows[0].Cells[KolStatus].Value?.ToString() ?? "";
            if (status == "LUNAS")
            {
                MessageBox.Show("Piutang ini sudah lunas.", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var form = new FormCatatBayarPiutang(id, nota, sisa, _koneksi);
            if (form.ShowDialog(this) == DialogResult.OK)
                MuatData();
        }

        private void DgvPiutang_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            // Tidak ada formatting tambahan; warna diatur di MuatData.
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // Sub-form: FormCatatBayarPiutang
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>Form untuk mencatat satu kali pembayaran piutang.</summary>
    internal class FormCatatBayarPiutang : Form
    {
        private readonly TextBox   txtJumlah   = new();
        private readonly ComboBox  cmbCaraBayar = new();
        private readonly TextBox   txtNoKuitansi = new();
        private readonly TextBox   txtKeterangan = new();
        private readonly Button    btnSimpan    = new();
        private readonly Button    btnBatal     = new();

        private readonly int     _idPiutang;
        private readonly Koneksi _koneksi;

        public FormCatatBayarPiutang(int idPiutang, string noNota, string sisaTeks, Koneksi koneksi)
        {
            _idPiutang = idPiutang;
            _koneksi   = koneksi;

            Text            = "Catat Bayar — " + noNota;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            MinimizeBox     = false;
            StartPosition   = FormStartPosition.CenterParent;
            ClientSize      = new Size(380, 260);
            Font            = new Font("Segoe UI", 9F);

            cmbCaraBayar.Items.AddRange(new object[] { "Tunai", "Transfer Bank", "Debit/Kredit", "QRIS" });
            cmbCaraBayar.SelectedIndex = 0;
            txtJumlah.Text = sisaTeks; // isi otomatis dengan sisa

            BangunTampilan();
        }

        private void BangunTampilan()
        {
            var tabel = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                ColumnCount = 2,
                RowCount    = 6,
                Padding     = new Padding(14)
            };
            tabel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
            tabel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            for (int i = 0; i < 5; i++)
                tabel.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            tabel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            foreach (TextBox t in new[] { txtJumlah, txtNoKuitansi, txtKeterangan })
                t.Dock = DockStyle.Fill;
            cmbCaraBayar.Dock = DockStyle.Fill;
            cmbCaraBayar.DropDownStyle = ComboBoxStyle.DropDownList;

            Tambah(tabel, "Jumlah Bayar*", txtJumlah,    0);
            Tambah(tabel, "Cara Bayar",    cmbCaraBayar, 1);
            Tambah(tabel, "No. Kuitansi",  txtNoKuitansi,2);
            Tambah(tabel, "Keterangan",    txtKeterangan,3);

            var panelTombol = new FlowLayoutPanel
            {
                Dock          = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Padding       = new Padding(0, 4, 0, 0)
            };
            Konfigurasi(btnSimpan, "Simpan", Color.FromArgb(39, 174, 96));
            Konfigurasi(btnBatal,  "Batal",  Color.FromArgb(150, 150, 150));
            panelTombol.Controls.AddRange(new Control[] { btnBatal, btnSimpan });
            tabel.Controls.Add(panelTombol, 0, 5);
            tabel.SetColumnSpan(panelTombol, 2);

            Controls.Add(tabel);
            btnSimpan.Click += BtnSimpan_Click;
            btnBatal.Click  += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
            AcceptButton     = btnSimpan;
        }

        private static void Tambah(TableLayoutPanel t, string label, Control ctrl, int baris)
        {
            t.Controls.Add(new Label { Text = label, TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill }, 0, baris);
            t.Controls.Add(ctrl, 1, baris);
        }

        private static void Konfigurasi(Button btn, string teks, Color warna)
        {
            btn.Text = teks; btn.Width = 90; btn.Height = 28;
            btn.FlatStyle = FlatStyle.Flat; btn.BackColor = warna; btn.ForeColor = Color.White;
            btn.FlatAppearance.BorderSize = 0;
        }

        private void BtnSimpan_Click(object? sender, EventArgs e)
        {
            if (!InputHelper.TryParseNominal(txtJumlah.Text, out decimal jumlah, out string pesan)
                || jumlah <= 0)
            {
                MessageBox.Show("Jumlah bayar tidak valid.\n" + pesan,
                    "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            jumlah = Math.Round(jumlah, 2);

            try
            {
                using SqliteConnection conn = _koneksi.GetConn();
                using var tr = conn.BeginTransaction(deferred: false);

                // Cek sisa piutang terbaru
                decimal jumlahPiutang, sudahBayar;
                using (SqliteCommand cmdCek = new(
                    "SELECT jumlah_piutang, sudah_bayar FROM tb_piutang WHERE id_piutang = @id", conn, tr))
                {
                    cmdCek.Parameters.AddWithValue("@id", _idPiutang);
                    using SqliteDataReader r = cmdCek.ExecuteReader();
                    if (!r.Read()) throw new Exception("Data piutang tidak ditemukan.");
                    jumlahPiutang = InputHelper.AmbilDecimal(r["jumlah_piutang"]);
                    sudahBayar    = InputHelper.AmbilDecimal(r["sudah_bayar"]);
                }

                decimal sisa = jumlahPiutang - sudahBayar;
                if (jumlah > sisa)
                {
                    MessageBox.Show($"Jumlah bayar ({InputHelper.FormatNominal(jumlah)}) melebihi sisa piutang ({InputHelper.FormatNominal(sisa)}).",
                        "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    tr.Rollback();
                    return;
                }

                // Catat pembayaran
                using (SqliteCommand cmdBayar = new(
                    "INSERT INTO tb_piutang_bayar (id_piutang, jumlah_bayar, cara_bayar, no_kuitansi, keterangan, id_user) "
                    + "VALUES (@id, @jml, @cara, @kuit, @ket, @user)", conn, tr))
                {
                    cmdBayar.Parameters.AddWithValue("@id",   _idPiutang);
                    cmdBayar.Parameters.AddWithValue("@jml",  jumlah);
                    cmdBayar.Parameters.AddWithValue("@cara", cmbCaraBayar.SelectedItem?.ToString() ?? "Tunai");
                    cmdBayar.Parameters.AddWithValue("@kuit", txtNoKuitansi.Text.Trim().Length > 0 ? txtNoKuitansi.Text.Trim() : DBNull.Value);
                    cmdBayar.Parameters.AddWithValue("@ket",  txtKeterangan.Text.Trim().Length > 0 ? txtKeterangan.Text.Trim() : DBNull.Value);
                    cmdBayar.Parameters.AddWithValue("@user", Session.UserId);
                    cmdBayar.ExecuteNonQuery();
                }

                // Update kolom sudah_bayar di tb_piutang
                using (SqliteCommand cmdUpd = new(
                    "UPDATE tb_piutang SET sudah_bayar = ROUND(sudah_bayar + @jml, 2) WHERE id_piutang = @id", conn, tr))
                {
                    cmdUpd.Parameters.AddWithValue("@jml", jumlah);
                    cmdUpd.Parameters.AddWithValue("@id",  _idPiutang);
                    cmdUpd.ExecuteNonQuery();
                }

                tr.Commit();

                MessageBox.Show("Pembayaran berhasil dicatat.", "Berhasil",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal menyimpan pembayaran:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
