using Microsoft.Data.Sqlite;

namespace AplikasiKasirSMK4
{
    /// <summary>
    /// Form untuk mengelola data member pelanggan.
    /// </summary>
    /// <remarks>
    /// Member bisa mendapatkan poin dari setiap transaksi dan bisa
    /// menggunakan poin sebagai potongan harga. Diskon member juga
    /// bisa diatur per-member lewat kolom diskon_persen.
    /// <para>
    /// Kode member dibuat otomatis oleh aplikasi dengan format
    /// MBR0001, MBR0002, dst. Admin bisa mengubah data member,
    /// tetapi kode member tidak bisa diubah setelah dibuat.
    /// </para>
    /// </remarks>
    public class FormMember : Form
    {
        // ─── Kontrol ────────────────────────────────────────────────
        private readonly DataGridView dgvMember = new();
        private readonly Button btnTambah    = new();
        private readonly Button btnEdit      = new();
        private readonly Button btnAktif     = new();
        private readonly Button btnRefresh   = new();
        private readonly TextBox txtCari     = new();
        private readonly Label lblSaldo      = new();
        private readonly CheckBox chkNonaktif = new();

        private readonly Koneksi _koneksi = new();

        // ─── Konstanta kolom ────────────────────────────────────────
        private const int KolId       = 0;
        private const int KolKode     = 1;
        private const int KolNama     = 2;
        private const int KolTelepon  = 3;
        private const int KolDiskon   = 4;
        private const int KolPoin     = 5;
        private const int KolAktif    = 6;
        private const int KolDibuat   = 7;

        public FormMember()
        {
            Text          = "Data Member";
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize   = new Size(820, 500);
            ClientSize    = new Size(870, 540);
            Font          = new Font("Segoe UI", 9F);

            BangunTampilan();
            Load += FormMember_Load;
        }

        // ─── BANGUN TAMPILAN ─────────────────────────────────────────

        private void BangunTampilan()
        {
            // Panel atas
            var panelAtas = new Panel
            {
                Dock    = DockStyle.Top,
                Height  = 90,
                Padding = new Padding(8)
            };

            KonfigTombol(btnTambah,  "＋ Tambah",  Color.FromArgb(39, 174, 96));
            KonfigTombol(btnEdit,    "✎ Edit",     Color.FromArgb(52, 152, 219));
            KonfigTombol(btnAktif,   "⊘ Nonaktif", Color.FromArgb(231, 76, 60));
            KonfigTombol(btnRefresh, "↻ Muat",     Color.FromArgb(100, 100, 100));

            var lblCari = new Label
            {
                Text      = "Cari:",
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize  = true,
                Location  = new Point(8, 54)
            };

            txtCari.Location = new Point(44, 50);
            txtCari.Width    = 220;

            chkNonaktif.Text     = "Tampilkan nonaktif";
            chkNonaktif.AutoSize = true;
            chkNonaktif.Location = new Point(275, 54);

            lblSaldo.AutoSize  = true;
            lblSaldo.ForeColor = Color.FromArgb(39, 174, 96);
            lblSaldo.Font      = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblSaldo.Location  = new Point(440, 54);

            // Baris tombol pertama
            int x = 8;
            foreach (Button b in new[] { btnTambah, btnEdit, btnAktif, btnRefresh })
            {
                b.Location = new Point(x, 10);
                x         += b.Width + 6;
            }

            panelAtas.Controls.AddRange(new Control[]
                { btnTambah, btnEdit, btnAktif, btnRefresh, lblCari, txtCari, chkNonaktif, lblSaldo });

            // DataGridView
            UiThemeHelper.FormatTabel(dgvMember);
            dgvMember.Dock                  = DockStyle.Fill;
            dgvMember.ReadOnly              = true;
            dgvMember.AllowUserToAddRows    = false;
            dgvMember.AllowUserToDeleteRows = false;
            dgvMember.AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill;

            dgvMember.Columns.Add(new DataGridViewTextBoxColumn { Name = "id_member",   HeaderText = "ID",       Visible = false });
            dgvMember.Columns.Add(new DataGridViewTextBoxColumn { Name = "kode_member", HeaderText = "Kode",     FillWeight = 12 });
            dgvMember.Columns.Add(new DataGridViewTextBoxColumn { Name = "nama_member", HeaderText = "Nama",     FillWeight = 25 });
            dgvMember.Columns.Add(new DataGridViewTextBoxColumn { Name = "telepon",     HeaderText = "Telepon",  FillWeight = 16 });
            dgvMember.Columns.Add(new DataGridViewTextBoxColumn { Name = "diskon",      HeaderText = "Diskon %", FillWeight = 10 });
            dgvMember.Columns.Add(new DataGridViewTextBoxColumn { Name = "poin",        HeaderText = "Poin",     FillWeight = 10 });
            dgvMember.Columns.Add(new DataGridViewTextBoxColumn { Name = "is_active",   HeaderText = "Status",   FillWeight = 10 });
            dgvMember.Columns.Add(new DataGridViewTextBoxColumn { Name = "dibuat_pada", HeaderText = "Terdaftar",FillWeight = 17 });

            Controls.Add(dgvMember);
            Controls.Add(panelAtas);

            btnTambah.Click    += BtnTambah_Click;
            btnEdit.Click      += BtnEdit_Click;
            btnAktif.Click     += BtnAktif_Click;
            btnRefresh.Click   += (_, _) => MuatData();
            txtCari.TextChanged += (_, _) => MuatData();
            chkNonaktif.CheckedChanged += (_, _) => MuatData();
            dgvMember.SelectionChanged += DgvMember_SelectionChanged;
        }

        private static void KonfigTombol(Button btn, string teks, Color warna)
        {
            btn.Text      = teks;
            btn.Width     = 110;
            btn.Height    = 30;
            btn.FlatStyle = FlatStyle.Flat;
            btn.BackColor = warna;
            btn.ForeColor = Color.White;
            btn.Cursor    = Cursors.Hand;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = ControlPaint.Light(warna, 0.15f);
        }

        // ─── LOAD DATA ───────────────────────────────────────────────

        private void FormMember_Load(object? sender, EventArgs e)
        {
            if (!Session.IsAdmin)
            {
                MessageBox.Show("Hanya Admin yang dapat mengakses halaman ini.",
                    "Akses Ditolak", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }
            MuatData();
        }

        private void MuatData()
        {
            dgvMember.Rows.Clear();

            string cari = txtCari.Text.Trim();

            // Saldo poin = SUM dari tb_poin_log, jumlah_poin positif untuk
            // TAMBAH dan negatif (sudah dicek di aplikasi) untuk PAKAI/HANGUS.
            string query =
                "SELECT m.id_member, m.kode_member, m.nama_member, m.telepon, "
              + "    m.diskon_persen, m.is_active, m.dibuat_pada, "
              + "    COALESCE((SELECT SUM(p.jumlah_poin) FROM tb_poin_log p "
              + "              WHERE p.id_member = m.id_member), 0) AS saldo_poin "
              + "FROM tb_member m "
              + "WHERE 1=1 ";

            if (!chkNonaktif.Checked)
                query += " AND m.is_active = 1 ";

            if (cari.Length > 0)
                query += " AND (m.nama_member LIKE @cari OR m.kode_member LIKE @cari OR m.telepon LIKE @cari) ";

            query += " ORDER BY m.nama_member";

            try
            {
                using SqliteConnection conn = _koneksi.GetConn();
                using SqliteCommand cmd = new(query, conn);
                if (cari.Length > 0)
                    cmd.Parameters.AddWithValue("@cari", "%" + cari + "%");

                using SqliteDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    int     id      = InputHelper.AmbilInt(reader["id_member"]);
                    string  kode    = reader["kode_member"]?.ToString() ?? "";
                    string  nama    = reader["nama_member"]?.ToString() ?? "";
                    string  telepon = reader["telepon"]?.ToString() ?? "";
                    decimal diskon  = InputHelper.AmbilDecimal(reader["diskon_persen"]);
                    decimal poin    = InputHelper.AmbilDecimal(reader["saldo_poin"]);
                    bool    aktif   = InputHelper.AmbilDecimal(reader["is_active"]) == 1m;
                    string  dibuat  = reader["dibuat_pada"]?.ToString() ?? "";

                    int baris = dgvMember.Rows.Add(
                        id, kode, nama, telepon,
                        diskon > 0 ? diskon.ToString("0.##") + "%" : "-",
                        (long)poin,
                        aktif ? "Aktif" : "Nonaktif",
                        dibuat);

                    if (!aktif)
                        dgvMember.Rows[baris].DefaultCellStyle.ForeColor = Color.Gray;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat data member:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            PerbaruitombolAktif();
        }

        // ─── EVENT TOMBOL ────────────────────────────────────────────

        private void BtnTambah_Click(object? sender, EventArgs e)
        {
            string kodeBaru = BuatKodeMemberBaru();
            using var form = new FormMemberEdit(null, kodeBaru, _koneksi);
            if (form.ShowDialog(this) == DialogResult.OK)
                MuatData();
        }

        private void BtnEdit_Click(object? sender, EventArgs e)
        {
            if (!AmbilIdTerpilih(out int id)) return;
            string kode = dgvMember.SelectedRows[0].Cells[KolKode].Value?.ToString() ?? "";
            using var form = new FormMemberEdit(id, kode, _koneksi);
            if (form.ShowDialog(this) == DialogResult.OK)
                MuatData();
        }

        private void BtnAktif_Click(object? sender, EventArgs e)
        {
            if (!AmbilIdTerpilih(out int id)) return;
            bool sedangAktif = dgvMember.SelectedRows[0].Cells[KolAktif].Value?.ToString() == "Aktif";
            string nama      = dgvMember.SelectedRows[0].Cells[KolNama].Value?.ToString() ?? "";

            var ok = MessageBox.Show(
                $"Yakin ingin {(sedangAktif ? "nonaktifkan" : "aktifkan")} member \"{nama}\"?",
                "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (ok != DialogResult.Yes) return;

            try
            {
                using SqliteConnection conn = _koneksi.GetConn();
                using SqliteCommand cmd = new(
                    "UPDATE tb_member SET is_active = @v, "
                    + "diperbarui_pada = strftime('%Y-%m-%d %H:%M:%S','now','localtime') "
                    + "WHERE id_member = @id", conn);
                cmd.Parameters.AddWithValue("@v", sedangAktif ? 0 : 1);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
                MuatData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal mengubah status:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ─── HELPER ─────────────────────────────────────────────────

        private bool AmbilIdTerpilih(out int id)
        {
            id = 0;
            if (dgvMember.SelectedRows.Count == 0)
            {
                MessageBox.Show("Pilih satu member terlebih dahulu.",
                    "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            id = InputHelper.AmbilInt(dgvMember.SelectedRows[0].Cells[KolId].Value);
            return true;
        }

        private void PerbaruitombolAktif()
        {
            bool ada = dgvMember.SelectedRows.Count > 0;
            btnEdit.Enabled = ada;
            btnAktif.Enabled = ada;

            if (ada)
            {
                bool aktif        = dgvMember.SelectedRows[0].Cells[KolAktif].Value?.ToString() == "Aktif";
                btnAktif.Text     = aktif ? "⊘ Nonaktif" : "✓ Aktifkan";
                btnAktif.BackColor = aktif ? Color.FromArgb(231, 76, 60) : Color.FromArgb(39, 174, 96);

                // Tampilkan saldo poin member terpilih
                decimal poin = InputHelper.AmbilDecimal(dgvMember.SelectedRows[0].Cells[KolPoin].Value);
                lblSaldo.Text = $"Poin: {(long)poin:N0}";
            }
            else
            {
                lblSaldo.Text = "";
            }
        }

        private void DgvMember_SelectionChanged(object? sender, EventArgs e) => PerbaruitombolAktif();

        /// <summary>Membuat kode member berikutnya dalam format MBR0001.</summary>
        private string BuatKodeMemberBaru()
        {
            try
            {
                using SqliteConnection conn = _koneksi.GetConn();
                using SqliteCommand cmd = new(
                    "SELECT kode_member FROM tb_member ORDER BY id_member DESC LIMIT 1", conn);
                string? terakhir = cmd.ExecuteScalar()?.ToString();
                if (terakhir is not null
                    && terakhir.StartsWith("MBR", StringComparison.OrdinalIgnoreCase)
                    && int.TryParse(terakhir[3..], out int nomor))
                {
                    return "MBR" + (nomor + 1).ToString("D4");
                }
            }
            catch { /* gunakan default */ }
            return "MBR0001";
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // Sub-form: FormMemberEdit (tambah / edit member)
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>Form isian data member — tambah baru atau ubah yang sudah ada.</summary>
    internal class FormMemberEdit : Form
    {
        private readonly TextBox  txtKode    = new();
        private readonly TextBox  txtNama    = new();
        private readonly TextBox  txtTelepon = new();
        private readonly TextBox  txtAlamat  = new();
        private readonly TextBox  txtEmail   = new();
        private readonly TextBox  txtDiskon  = new();
        private readonly TextBox  txtPoinAwal = new();
        private readonly Button   btnSimpan  = new();
        private readonly Button   btnBatal   = new();

        private readonly int? _idMember;
        private readonly Koneksi _koneksi;

        public FormMemberEdit(int? idMember, string kode, Koneksi koneksi)
        {
            _idMember = idMember;
            _koneksi  = koneksi;

            Text            = idMember.HasValue ? "Edit Member" : "Tambah Member Baru";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            MinimizeBox     = false;
            StartPosition   = FormStartPosition.CenterParent;
            ClientSize      = new Size(400, 380);
            Font            = new Font("Segoe UI", 9F);

            txtKode.Text     = kode;
            txtKode.ReadOnly = true; // kode tidak bisa diubah setelah dibuat
            txtKode.BackColor = Color.FromArgb(240, 240, 240);

            BangunTampilan();
            Load += FormMemberEdit_Load;
        }

        private void BangunTampilan()
        {
            var tabel = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                ColumnCount = 2,
                RowCount    = 9,
                Padding     = new Padding(14)
            };
            tabel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            tabel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            for (int i = 0; i < 8; i++)
                tabel.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            tabel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            Tambah(tabel, "Kode",        txtKode,    0);
            Tambah(tabel, "Nama*",       txtNama,    1);
            Tambah(tabel, "Telepon",     txtTelepon, 2);
            Tambah(tabel, "Email",       txtEmail,   3);
            Tambah(tabel, "Alamat",      txtAlamat,  4);
            Tambah(tabel, "Diskon (%)",  txtDiskon,  5);

            if (!_idMember.HasValue)
            {
                Tambah(tabel, "Poin Awal", txtPoinAwal, 6);
                tabel.Controls.Add(new Label
                {
                    Text      = "Poin awal (bisa kosong)",
                    ForeColor = Color.Gray,
                    Font      = new Font("Segoe UI", 7.5F),
                    Dock      = DockStyle.Fill
                }, 1, 7);
            }

            foreach (TextBox t in new[] { txtNama, txtTelepon, txtEmail, txtAlamat, txtDiskon, txtPoinAwal })
                t.Dock = DockStyle.Fill;

            var panelTombol = new FlowLayoutPanel
            {
                Dock          = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Padding       = new Padding(0, 4, 0, 0)
            };

            Konfigurasi(btnSimpan, "Simpan", Color.FromArgb(39, 174, 96));
            Konfigurasi(btnBatal,  "Batal",  Color.FromArgb(150, 150, 150));
            panelTombol.Controls.AddRange(new Control[] { btnBatal, btnSimpan });
            tabel.Controls.Add(panelTombol, 0, 8);
            tabel.SetColumnSpan(panelTombol, 2);

            Controls.Add(tabel);

            btnSimpan.Click += BtnSimpan_Click;
            btnBatal.Click  += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
            AcceptButton     = btnSimpan;
        }

        private static void Tambah(TableLayoutPanel t, string label, Control ctrl, int baris)
        {
            t.Controls.Add(new Label
            {
                Text      = label,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock      = DockStyle.Fill
            }, 0, baris);
            t.Controls.Add(ctrl, 1, baris);
        }

        private static void Konfigurasi(Button btn, string teks, Color warna)
        {
            btn.Text      = teks;
            btn.Width     = 90;
            btn.Height    = 28;
            btn.FlatStyle = FlatStyle.Flat;
            btn.BackColor = warna;
            btn.ForeColor = Color.White;
            btn.FlatAppearance.BorderSize = 0;
        }

        private void FormMemberEdit_Load(object? sender, EventArgs e)
        {
            if (!_idMember.HasValue) return;

            try
            {
                using SqliteConnection conn = _koneksi.GetConn();
                using SqliteCommand cmd = new(
                    "SELECT kode_member, nama_member, telepon, email, alamat, diskon_persen "
                    + "FROM tb_member WHERE id_member = @id", conn);
                cmd.Parameters.AddWithValue("@id", _idMember.Value);
                using SqliteDataReader r = cmd.ExecuteReader();
                if (r.Read())
                {
                    txtKode.Text    = r["kode_member"]?.ToString() ?? "";
                    txtNama.Text    = r["nama_member"]?.ToString() ?? "";
                    txtTelepon.Text = r["telepon"]?.ToString() ?? "";
                    txtEmail.Text   = r["email"]?.ToString() ?? "";
                    txtAlamat.Text  = r["alamat"]?.ToString() ?? "";
                    decimal dis     = InputHelper.AmbilDecimal(r["diskon_persen"]);
                    txtDiskon.Text  = dis > 0 ? dis.ToString("0.##") : "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat data:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSimpan_Click(object? sender, EventArgs e)
        {
            string nama    = txtNama.Text.Trim();
            string telepon = txtTelepon.Text.Trim();
            string email   = txtEmail.Text.Trim();
            string alamat  = txtAlamat.Text.Trim();

            if (nama.Length == 0)
            {
                MessageBox.Show("Nama member tidak boleh kosong.",
                    "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal diskon = 0m;
            if (txtDiskon.Text.Trim().Length > 0)
            {
                if (!InputHelper.TryParseNominal(txtDiskon.Text, out diskon, out string pesanDiskon)
                    || diskon < 0 || diskon > 100)
                {
                    MessageBox.Show("Diskon harus angka antara 0 dan 100.\n" + pesanDiskon,
                        "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                diskon = Math.Round(diskon, 2);
            }

            try
            {
                using SqliteConnection conn = _koneksi.GetConn();

                if (_idMember.HasValue)
                {
                    // Edit
                    using SqliteCommand cmd = new(
                        "UPDATE tb_member SET nama_member=@nama, telepon=@tel, email=@email, "
                        + "alamat=@alamat, diskon_persen=@diskon, "
                        + "diperbarui_pada=strftime('%Y-%m-%d %H:%M:%S','now','localtime') "
                        + "WHERE id_member=@id", conn);
                    cmd.Parameters.AddWithValue("@nama",   nama);
                    cmd.Parameters.AddWithValue("@tel",    telepon.Length > 0 ? telepon : DBNull.Value);
                    cmd.Parameters.AddWithValue("@email",  email.Length > 0   ? email   : DBNull.Value);
                    cmd.Parameters.AddWithValue("@alamat", alamat.Length > 0  ? alamat  : DBNull.Value);
                    cmd.Parameters.AddWithValue("@diskon", diskon);
                    cmd.Parameters.AddWithValue("@id",     _idMember.Value);
                    cmd.ExecuteNonQuery();
                }
                else
                {
                    // Tambah baru
                    using SqliteCommand cmd = new(
                        "INSERT INTO tb_member (kode_member, nama_member, telepon, email, alamat, diskon_persen, is_active) "
                        + "VALUES (@kode, @nama, @tel, @email, @alamat, @diskon, 1)", conn);
                    cmd.Parameters.AddWithValue("@kode",   txtKode.Text);
                    cmd.Parameters.AddWithValue("@nama",   nama);
                    cmd.Parameters.AddWithValue("@tel",    telepon.Length > 0 ? telepon : DBNull.Value);
                    cmd.Parameters.AddWithValue("@email",  email.Length > 0   ? email   : DBNull.Value);
                    cmd.Parameters.AddWithValue("@alamat", alamat.Length > 0  ? alamat  : DBNull.Value);
                    cmd.Parameters.AddWithValue("@diskon", diskon);
                    cmd.ExecuteNonQuery();

                    // Catat poin awal jika ada
                    string poinTeks = txtPoinAwal.Text.Trim();
                    if (poinTeks.Length > 0
                        && int.TryParse(poinTeks, out int poinAwal)
                        && poinAwal > 0)
                    {
                        long idBaru;
                        using SqliteCommand cmdId = new("SELECT last_insert_rowid()", conn);
                        idBaru = Convert.ToInt64(cmdId.ExecuteScalar());

                        using SqliteCommand cmdPoin = new(
                            "INSERT INTO tb_poin_log (id_member, tipe, jumlah_poin, keterangan, id_user) "
                            + "VALUES (@m, 'TAMBAH', @poin, 'Poin awal pendaftaran', @u)", conn);
                        cmdPoin.Parameters.AddWithValue("@m",    idBaru);
                        cmdPoin.Parameters.AddWithValue("@poin", poinAwal);
                        cmdPoin.Parameters.AddWithValue("@u",    Session.UserId);
                        cmdPoin.ExecuteNonQuery();
                    }
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (SqliteException ex) when (ex.Message.Contains("UNIQUE"))
            {
                MessageBox.Show("Nama member atau kode tersebut sudah ada.",
                    "Duplikat", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal menyimpan:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
