using Microsoft.Data.Sqlite;

namespace AplikasiKasirSMK4
{
    /// <summary>
    /// Form untuk melihat stok semua barang dan melakukan opname (stock counting).
    /// </summary>
    /// <remarks>
    /// Ada dua tab:
    /// <list type="bullet">
    /// <item>Tab Stok: menampilkan seluruh barang dengan stok, harga, dan nilai persediaan.</item>
    /// <item>Tab Opname: menghitung fisik barang dan menerapkan selisih ke database.</item>
    /// </list>
    /// </remarks>
    public class FormStok : Form
    {
        private readonly TabControl tabControl = new();
        private readonly TabPage tabStok       = new();
        private readonly TabPage tabOpname     = new();

        private readonly Koneksi _koneksi = new();

        public FormStok()
        {
            Text          = "Stok Barang";
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize   = new Size(900, 560);
            ClientSize    = new Size(940, 580);
            Font          = new Font("Segoe UI", 9F);

            tabControl.Dock = DockStyle.Fill;
            tabControl.TabPages.Add(tabStok);
            tabControl.TabPages.Add(tabOpname);
            Controls.Add(tabControl);

            IsiTabStok();
            IsiTabOpname();

            Load += FormStok_Load;
        }

        // ─── TAB STOK ────────────────────────────────────────────────

        private DataGridView dgvStok     = new();
        private TextBox      txtCariStok = new();
        private Button       btnRefreshStok = new();
        private Label        lblNilaiPersediaan = new();

        private void IsiTabStok()
        {
            tabStok.Text    = "📦 Stok Saat Ini";
            tabStok.Padding = new Padding(6);

            var panelAtas = new Panel { Dock = DockStyle.Top, Height = 44 };
            var lblCari   = new Label { Text = "Cari:", AutoSize = true, Location = new Point(4, 12) };
            txtCariStok.Location = new Point(40, 8);
            txtCariStok.Width    = 220;
            KonfigTombol(btnRefreshStok, "↻ Muat", Color.FromArgb(100, 100, 100));
            btnRefreshStok.Location = new Point(270, 7);
            lblNilaiPersediaan.AutoSize  = true;
            lblNilaiPersediaan.Font      = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblNilaiPersediaan.ForeColor = Color.FromArgb(39, 174, 96);
            lblNilaiPersediaan.Location  = new Point(400, 12);
            panelAtas.Controls.AddRange(new Control[] { lblCari, txtCariStok, btnRefreshStok, lblNilaiPersediaan });

            UiThemeHelper.FormatTabel(dgvStok);
            dgvStok.Dock                  = DockStyle.Fill;
            dgvStok.ReadOnly              = true;
            dgvStok.AllowUserToAddRows    = false;
            dgvStok.AllowUserToDeleteRows = false;
            dgvStok.AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill;

            dgvStok.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Barcode",   FillWeight = 15 });
            dgvStok.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Nama Barang", FillWeight = 30 });
            dgvStok.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Satuan",    FillWeight = 8 });
            dgvStok.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Stok",      FillWeight = 8,  DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight } });
            dgvStok.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Harga Beli",FillWeight = 14, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight } });
            dgvStok.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Harga Jual",FillWeight = 14, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight } });
            dgvStok.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Nilai Persediaan", FillWeight = 16, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight } });

            tabStok.Controls.Add(dgvStok);
            tabStok.Controls.Add(panelAtas);

            btnRefreshStok.Click  += (_, _) => MuatDataStok();
            txtCariStok.TextChanged += (_, _) => MuatDataStok();
        }

        private void MuatDataStok()
        {
            dgvStok.Rows.Clear();
            string cari = txtCariStok.Text.Trim();

            string query =
                "SELECT kode_barcode, nama_barang, satuan, stok, harga_beli, harga_jual, "
              + "    ROUND(stok * harga_beli, 2) AS nilai_persediaan "
              + "FROM tb_barang WHERE is_active = 1 ";
            if (cari.Length > 0)
                query += " AND (nama_barang LIKE @cari OR kode_barcode LIKE @cari) ";
            query += " ORDER BY nama_barang";

            decimal totalNilai = 0m;

            try
            {
                using SqliteConnection conn = _koneksi.GetConn();
                using SqliteCommand cmd = new(query, conn);
                if (cari.Length > 0)
                    cmd.Parameters.AddWithValue("@cari", "%" + cari + "%");

                using SqliteDataReader r = cmd.ExecuteReader();
                while (r.Read())
                {
                    string  kode   = r["kode_barcode"]?.ToString() ?? "";
                    string  nama   = r["nama_barang"]?.ToString() ?? "";
                    string  satuan = r["satuan"]?.ToString() ?? "";
                    decimal stok   = InputHelper.AmbilDecimal(r["stok"]);
                    decimal beli   = InputHelper.AmbilDecimal(r["harga_beli"]);
                    decimal jual   = InputHelper.AmbilDecimal(r["harga_jual"]);
                    decimal nilai  = InputHelper.AmbilDecimal(r["nilai_persediaan"]);

                    int baris = dgvStok.Rows.Add(kode, nama, satuan,
                        InputHelper.FormatJumlah(stok),
                        InputHelper.FormatNominal(beli),
                        InputHelper.FormatNominal(jual),
                        InputHelper.FormatNominal(nilai));

                    // Warnai stok nol atau negatif
                    if (stok <= 0)
                        dgvStok.Rows[baris].DefaultCellStyle.BackColor = Color.FromArgb(255, 235, 235);
                    else if (stok < 5)
                        dgvStok.Rows[baris].DefaultCellStyle.BackColor = Color.FromArgb(255, 252, 210);

                    totalNilai += nilai;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat stok:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            lblNilaiPersediaan.Text = "Nilai persediaan: " + InputHelper.FormatNominal(totalNilai);
        }

        // ─── TAB OPNAME ──────────────────────────────────────────────

        private DataGridView dgvOpname    = new();
        private Button       btnMuatBarang = new();
        private Button       btnTerapkan   = new();
        private TextBox      txtCatatanOpname = new();
        private Label        lblNomorOpname   = new();

        private void IsiTabOpname()
        {
            tabOpname.Text    = "🔢 Opname Stok";
            tabOpname.Padding = new Padding(6);

            var panelAtas = new Panel { Dock = DockStyle.Top, Height = 80 };

            var lblNomor = new Label { Text = "Nomor:", AutoSize = true, Location = new Point(4, 12) };
            lblNomorOpname.AutoSize = true;
            lblNomorOpname.Font     = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblNomorOpname.Location = new Point(60, 12);

            var lblCatatan = new Label { Text = "Catatan:", AutoSize = true, Location = new Point(4, 46) };
            txtCatatanOpname.Location = new Point(64, 42);
            txtCatatanOpname.Width    = 300;

            KonfigTombol(btnMuatBarang, "📋 Muat Semua Barang", Color.FromArgb(52, 152, 219));
            btnMuatBarang.Location = new Point(380, 8);
            KonfigTombol(btnTerapkan,   "✔ Terapkan Opname", Color.FromArgb(39, 174, 96));
            btnTerapkan.Location   = new Point(380, 44);

            panelAtas.Controls.AddRange(new Control[]
                { lblNomor, lblNomorOpname, lblCatatan, txtCatatanOpname, btnMuatBarang, btnTerapkan });

            UiThemeHelper.FormatTabel(dgvOpname);
            dgvOpname.Dock                  = DockStyle.Fill;
            dgvOpname.AllowUserToAddRows    = false;
            dgvOpname.AllowUserToDeleteRows = false;
            dgvOpname.AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill;

            dgvOpname.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Barcode",    FillWeight = 14, ReadOnly = true });
            dgvOpname.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Nama",       FillWeight = 28, ReadOnly = true });
            dgvOpname.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Stok Sistem",FillWeight = 12, ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight } });
            dgvOpname.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Stok Fisik", FillWeight = 12,
                DefaultCellStyle = new DataGridViewCellStyle {
                    BackColor = Color.FromArgb(255, 255, 220),
                    Alignment = DataGridViewContentAlignment.MiddleRight
                } });
            dgvOpname.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Selisih",    FillWeight = 12, ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight } });
            dgvOpname.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Harga Beli", FillWeight = 12, ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight } });
            dgvOpname.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Nilai Selisih", FillWeight = 14, ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight } });

            dgvOpname.CellEndEdit += DgvOpname_CellEndEdit;

            tabOpname.Controls.Add(dgvOpname);
            tabOpname.Controls.Add(panelAtas);

            btnMuatBarang.Click += BtnMuatBarang_Click;
            btnTerapkan.Click   += BtnTerapkan_Click;
        }

        private void BtnMuatBarang_Click(object? sender, EventArgs e)
        {
            dgvOpname.Rows.Clear();
            lblNomorOpname.Text = "OPN-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");

            try
            {
                using SqliteConnection conn = _koneksi.GetConn();
                using SqliteCommand cmd = new(
                    "SELECT kode_barcode, nama_barang, stok, harga_beli FROM tb_barang "
                    + "WHERE is_active = 1 ORDER BY nama_barang", conn);
                using SqliteDataReader r = cmd.ExecuteReader();
                while (r.Read())
                {
                    decimal stok  = InputHelper.AmbilDecimal(r["stok"]);
                    decimal beli  = InputHelper.AmbilDecimal(r["harga_beli"]);
                    dgvOpname.Rows.Add(
                        r["kode_barcode"]?.ToString() ?? "",
                        r["nama_barang"]?.ToString() ?? "",
                        InputHelper.FormatJumlah(stok),
                        InputHelper.FormatJumlah(stok),  // fisik awal = sistem
                        "0",
                        InputHelper.FormatNominal(beli),
                        "0");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat barang:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvOpname_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
        {
            // Kolom stok fisik = indeks 3
            if (e.ColumnIndex != 3) return;

            var row        = dgvOpname.Rows[e.RowIndex];
            string sisText = row.Cells[2].Value?.ToString() ?? "0"; // stok sistem
            string fisText = row.Cells[3].Value?.ToString() ?? "0"; // stok fisik

            if (!decimal.TryParse(sisText.Replace(".", "").Replace(",", "."),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out decimal sis))
                sis = 0m;
            if (!decimal.TryParse(fisText.Replace(".", "").Replace(",", "."),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out decimal fis))
                fis = 0m;

            decimal selisih = fis - sis;
            string beliText = row.Cells[5].Value?.ToString() ?? "0";
            if (!decimal.TryParse(beliText.Replace(".", "").Replace(",", "."),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out decimal beli))
                beli = 0m;

            decimal nilaiSelisih = Math.Round(selisih * beli, 2);

            row.Cells[4].Value = InputHelper.FormatJumlah(selisih);
            row.Cells[6].Value = InputHelper.FormatNominal(nilaiSelisih);

            // Warnai selisih
            if (selisih < 0)
                row.Cells[4].Style.ForeColor = Color.FromArgb(192, 0, 0);
            else if (selisih > 0)
                row.Cells[4].Style.ForeColor = Color.FromArgb(39, 130, 60);
            else
                row.Cells[4].Style.ForeColor = Color.Black;
        }

        private void BtnTerapkan_Click(object? sender, EventArgs e)
        {
            if (dgvOpname.Rows.Count == 0)
            {
                MessageBox.Show("Muat barang terlebih dahulu.", "Peringatan",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string nomorOpname = lblNomorOpname.Text;
            string catatan     = txtCatatanOpname.Text.Trim();

            var ok = MessageBox.Show(
                "Terapkan hasil opname?\n\nStok semua barang akan disesuaikan dengan stok fisik yang diisi.",
                "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (ok != DialogResult.Yes) return;

            try
            {
                using SqliteConnection conn = _koneksi.GetConn();
                using var tr = conn.BeginTransaction(deferred: false);

                // Simpan header opname
                using SqliteCommand cmdHeader = new(
                    "INSERT INTO tb_opname (nomor_opname, id_user, catatan, jumlah_item, selisih_nilai, sudah_diperapkan) "
                    + "VALUES (@nom, @usr, @cat, @jml, @sel, 1)", conn, tr);
                cmdHeader.Parameters.AddWithValue("@nom", nomorOpname);
                cmdHeader.Parameters.AddWithValue("@usr", Session.UserId);
                cmdHeader.Parameters.AddWithValue("@cat", catatan.Length > 0 ? catatan : DBNull.Value);
                cmdHeader.Parameters.AddWithValue("@jml", dgvOpname.Rows.Count);

                // Hitung total selisih nilai
                decimal totalSelisihNilai = 0m;
                foreach (DataGridViewRow baris in dgvOpname.Rows)
                {
                    string nilai = baris.Cells[6].Value?.ToString() ?? "0";
                    if (decimal.TryParse(nilai.Replace(".", "").Replace(",", "."),
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out decimal n))
                        totalSelisihNilai += n;
                }
                cmdHeader.Parameters.AddWithValue("@sel", Math.Round(totalSelisihNilai, 2));
                cmdHeader.ExecuteNonQuery();

                long idOpname;
                using (SqliteCommand cmdId = new("SELECT last_insert_rowid()", conn, tr))
                    idOpname = Convert.ToInt64(cmdId.ExecuteScalar());

                // Proses setiap baris
                foreach (DataGridViewRow baris in dgvOpname.Rows)
                {
                    string kode    = baris.Cells[0].Value?.ToString() ?? "";
                    string sisText = baris.Cells[2].Value?.ToString() ?? "0";
                    string fisText = baris.Cells[3].Value?.ToString() ?? "0";
                    string beliText = baris.Cells[5].Value?.ToString() ?? "0";

                    decimal.TryParse(sisText.Replace(".", "").Replace(",", "."),
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out decimal sis);
                    decimal.TryParse(fisText.Replace(".", "").Replace(",", "."),
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out decimal fis);
                    decimal.TryParse(beliText.Replace(".", "").Replace(",", "."),
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out decimal beli);

                    decimal selisih   = fis - sis;
                    decimal nilaiSel  = Math.Round(selisih * beli, 2);
                    string  namaBarang = baris.Cells[1].Value?.ToString() ?? "";

                    // Simpan detail opname
                    using (SqliteCommand cmdDetail = new(
                        "INSERT INTO tb_opname_detail "
                        + "(id_opname, kode_barcode, nama_barang, stok_sistem, stok_fisik, selisih, harga_beli, nilai_selisih) "
                        + "VALUES (@id, @kode, @nama, @sis, @fis, @sel, @beli, @nilai)", conn, tr))
                    {
                        cmdDetail.Parameters.AddWithValue("@id",    idOpname);
                        cmdDetail.Parameters.AddWithValue("@kode",  kode);
                        cmdDetail.Parameters.AddWithValue("@nama",  namaBarang);
                        cmdDetail.Parameters.AddWithValue("@sis",   sis);
                        cmdDetail.Parameters.AddWithValue("@fis",   fis);
                        cmdDetail.Parameters.AddWithValue("@sel",   selisih);
                        cmdDetail.Parameters.AddWithValue("@beli",  beli);
                        cmdDetail.Parameters.AddWithValue("@nilai", nilaiSel);
                        cmdDetail.ExecuteNonQuery();
                    }

                    // Update stok barang hanya jika ada selisih
                    if (selisih != 0m)
                    {
                        using SqliteCommand cmdUpd = new(
                            "UPDATE tb_barang SET stok = @stokBaru WHERE kode_barcode = @kode", conn, tr);
                        cmdUpd.Parameters.AddWithValue("@stokBaru", fis);
                        cmdUpd.Parameters.AddWithValue("@kode",     kode);
                        cmdUpd.ExecuteNonQuery();

                        // Catat di tb_mutasi_stok
                        using SqliteCommand cmdMutasi = new(
                            "INSERT INTO tb_mutasi_stok (kode_barcode, tipe, qty, stok_akhir, keterangan, id_user) "
                            + "VALUES (@kode, 'PENYESUAIAN', @qty, @akhir, @ket, @usr)", conn, tr);
                        cmdMutasi.Parameters.AddWithValue("@kode",  kode);
                        cmdMutasi.Parameters.AddWithValue("@qty",   selisih);
                        cmdMutasi.Parameters.AddWithValue("@akhir", fis);
                        cmdMutasi.Parameters.AddWithValue("@ket",   "Opname " + nomorOpname);
                        cmdMutasi.Parameters.AddWithValue("@usr",   Session.UserId);
                        cmdMutasi.ExecuteNonQuery();
                    }
                }

                tr.Commit();

                MessageBox.Show("Opname berhasil diterapkan!\n\nNomor: " + nomorOpname,
                    "Berhasil", MessageBoxButtons.OK, MessageBoxIcon.Information);

                dgvOpname.Rows.Clear();
                lblNomorOpname.Text = "";
                txtCatatanOpname.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal menerapkan opname:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ─── HELPER ─────────────────────────────────────────────────

        private static void KonfigTombol(Button btn, string teks, Color warna)
        {
            btn.Text      = teks;
            btn.Width     = 170;
            btn.Height    = 28;
            btn.FlatStyle = FlatStyle.Flat;
            btn.BackColor = warna;
            btn.ForeColor = Color.White;
            btn.Cursor    = Cursors.Hand;
            btn.FlatAppearance.BorderSize = 0;
        }

        private void FormStok_Load(object? sender, EventArgs e)
        {
            if (!Session.IsAdmin)
            {
                MessageBox.Show("Hanya Admin yang dapat mengakses halaman ini.",
                    "Akses Ditolak", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }
            MuatDataStok();
        }
    }
}
