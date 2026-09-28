using System.Data;
using Microsoft.Data.Sqlite;

namespace AplikasiKasirSMK4
{
    public partial class FormBarang : Form
    {
        private readonly Koneksi _koneksi = new();

        private const string SatuanBawaan = "pcs";
        private const decimal BatasStokMaksimum = 9999999999999.99m;

        private sealed class MasterComboItem
        {
            public int? Id { get; }
            public string Teks { get; }

            public MasterComboItem(int? id, string teks)
            {
                Id = id;
                Teks = teks;
            }

            public override string ToString() => Teks;
        }

        public FormBarang()
        {
            InitializeComponent();
            UiThemeHelper.FormatTabel(dgvBarang);
            dgvBarang.CellFormatting += dgvBarang_CellFormatting;
            txtStok.KeyPress += txtStok_KeyPress;
            txtMinGrosir.KeyPress += txtStok_KeyPress;
            txtDiskon.KeyPress += (s, e) =>
            {
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.' && e.KeyChar != ',')
                {
                    e.Handled = true;
                }
            };
        }

        private void txtStok_KeyPress(object? sender, KeyPressEventArgs e)
        {
            e.Handled = !InputHelper.BolehMasukAngka(e.KeyChar);
        }

        private void FormBarang_Load(object sender, EventArgs e)
        {
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

            cmbSatuan.Text = SatuanBawaan;
            MuatMasterDropdown();
            TampilData();
        }

        private void MuatMasterDropdown()
        {
            // 1. Jenis Barang
            cmbJenis.Items.Clear();
            cmbJenis.Items.Add(new MasterComboItem(null, "— Tanpa Jenis —"));
            try
            {
                using SqliteConnection conn = _koneksi.GetConn();
                using (SqliteCommand cmd = new("SELECT id_jenis, nama_jenis FROM tb_jenis_barang WHERE is_active = 1 ORDER BY nama_jenis", conn))
                using (SqliteDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        cmbJenis.Items.Add(new MasterComboItem(InputHelper.AmbilInt(r["id_jenis"]), r["nama_jenis"]?.ToString() ?? ""));
                    }
                }

                // 2. Merek Barang
                cmbMerek.Items.Clear();
                cmbMerek.Items.Add(new MasterComboItem(null, "— Tanpa Merek —"));
                using (SqliteCommand cmd = new("SELECT id_merek, nama_merek FROM tb_merek WHERE is_active = 1 ORDER BY nama_merek", conn))
                using (SqliteDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        cmbMerek.Items.Add(new MasterComboItem(InputHelper.AmbilInt(r["id_merek"]), r["nama_merek"]?.ToString() ?? ""));
                    }
                }

                // 3. Supplier
                cmbSupplier.Items.Clear();
                cmbSupplier.Items.Add(new MasterComboItem(null, "— Tanpa Supplier —"));
                using (SqliteCommand cmd = new("SELECT id_supplier, nama_supplier FROM tb_supplier WHERE is_active = 1 ORDER BY nama_supplier", conn))
                using (SqliteDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        cmbSupplier.Items.Add(new MasterComboItem(InputHelper.AmbilInt(r["id_supplier"]), r["nama_supplier"]?.ToString() ?? ""));
                    }
                }
            }
            catch { /* biarkan default */ }

            if (cmbJenis.Items.Count > 0) cmbJenis.SelectedIndex = 0;
            if (cmbMerek.Items.Count > 0) cmbMerek.SelectedIndex = 0;
            if (cmbSupplier.Items.Count > 0) cmbSupplier.SelectedIndex = 0;
        }

        // ------------------------------------------------------------------
        // READ
        // ------------------------------------------------------------------
        private void TampilData()
        {
            try
            {
                using SqliteConnection conn = _koneksi.GetConn();

                const string query =
                    "SELECT b.kode_barcode AS 'Kode', b.nama_barang AS 'Nama Barang', "
                    + "j.nama_jenis AS 'Jenis', m.nama_merek AS 'Merek', s.nama_supplier AS 'Supplier', "
                    + "b.satuan AS 'Satuan', b.harga_beli AS 'Harga Beli', "
                    + "b.harga_jual AS 'Harga Jual', b.diskon_persen AS 'Diskon (%)', "
                    + "b.minimal_grosir AS 'Min. Grosir', b.harga_grosir AS 'Harga Grosir', b.stok AS 'Stok', "
                    + "b.id_jenis, b.id_merek, b.id_supplier "
                    + "FROM tb_barang b "
                    + "LEFT JOIN tb_jenis_barang j ON j.id_jenis = b.id_jenis "
                    + "LEFT JOIN tb_merek m ON m.id_merek = b.id_merek "
                    + "LEFT JOIN tb_supplier s ON s.id_supplier = b.id_supplier "
                    + "WHERE b.is_active = 1 ORDER BY b.nama_barang ASC";

                DataTable dt = QueryHelper.IsiTabel(query, conn);
                dgvBarang.DataSource = dt;

                // Sembunyikan ID relasi dari tampilan grid
                if (dgvBarang.Columns["id_jenis"] is not null) dgvBarang.Columns["id_jenis"]!.Visible = false;
                if (dgvBarang.Columns["id_merek"] is not null) dgvBarang.Columns["id_merek"]!.Visible = false;
                if (dgvBarang.Columns["id_supplier"] is not null) dgvBarang.Columns["id_supplier"]!.Visible = false;
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
            cmbSatuan.Text = SatuanBawaan;
            txtHargaBeli.Text = string.Empty;
            txtHargaJual.Text = string.Empty;
            txtMinGrosir.Text = string.Empty;
            txtHargaGrosir.Text = string.Empty;
            txtDiskon.Text = string.Empty;
            txtStok.Text = string.Empty;
            if (cmbJenis.Items.Count > 0) cmbJenis.SelectedIndex = 0;
            if (cmbMerek.Items.Count > 0) cmbMerek.SelectedIndex = 0;
            if (cmbSupplier.Items.Count > 0) cmbSupplier.SelectedIndex = 0;
            txtKode.Enabled = true;
            txtKode.Focus();
        }

        private bool AmbilInput(out string kode, out string nama, out string satuan,
            out decimal hargaBeli, out decimal hargaJual, out decimal minGrosir, out decimal hargaGrosir,
            out decimal diskonPersen, out decimal stok, out int? idJenis, out int? idMerek, out int? idSupplier)
        {
            kode = txtKode.Text.Trim();
            nama = txtNama.Text.Trim();
            satuan = cmbSatuan.Text.Trim();
            if (satuan.Length == 0) satuan = SatuanBawaan;

            hargaBeli = 0m;
            hargaJual = 0m;
            minGrosir = 0m;
            hargaGrosir = 0m;
            diskonPersen = 0m;
            stok = 0m;

            idJenis = (cmbJenis.SelectedItem as MasterComboItem)?.Id;
            idMerek = (cmbMerek.SelectedItem as MasterComboItem)?.Id;
            idSupplier = (cmbSupplier.SelectedItem as MasterComboItem)?.Id;

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

            // Harga beli opsional
            if (txtHargaBeli.Text.Trim().Length > 0 && InputHelper.TryParseNominal(txtHargaBeli.Text, out decimal hb, out string pesanBeli))
            {
                hargaBeli = hb;
            }

            // Grosir opsional
            if (txtMinGrosir.Text.Trim().Length > 0 && InputHelper.TryParseBilanBulat(txtMinGrosir.Text, out int mg, out _))
            {
                minGrosir = mg;
            }
            if (txtHargaGrosir.Text.Trim().Length > 0 && InputHelper.TryParseNominal(txtHargaGrosir.Text, out decimal hg, out _))
            {
                hargaGrosir = hg;
            }

            // Diskon barang opsional (0 - 100%)
            if (txtDiskon.Text.Trim().Length > 0)
            {
                if (!InputHelper.TryParseNominal(txtDiskon.Text, out decimal dp, out string pesanDiskon)
                    || dp < 0 || dp > 100)
                {
                    MessageBox.Show("Diskon barang harus berupa angka antara 0 sampai 100%!\n" + pesanDiskon,
                        "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtDiskon.Focus();
                    return false;
                }
                diskonPersen = Math.Round(dp, 2);
            }

            if (!InputHelper.TryParseBilanBulat(txtStok.Text, out int stokBulat, out string pesanStok))
            {
                MessageBox.Show("Jumlah stok tidak valid: " + pesanStok + ".", "Peringatan",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtStok.Focus();
                return false;
            }

            if (stokBulat < 0)
            {
                MessageBox.Show("Jumlah stok tidak boleh negatif!", "Peringatan",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtStok.Focus();
                return false;
            }

            if (stokBulat > BatasStokMaksimum)
            {
                MessageBox.Show("Jumlah stok terlalu besar!", "Peringatan",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtStok.Focus();
                return false;
            }

            stok = stokBulat;
            return true;
        }

        // ------------------------------------------------------------------
        // CREATE
        // ------------------------------------------------------------------
        private void btnSimpan_Click(object sender, EventArgs e)
        {
            if (!AmbilInput(out string kode, out string nama, out string satuan,
                    out decimal hargaBeli, out decimal hargaJual, out decimal minGrosir, out decimal hargaGrosir,
                    out decimal diskonPersen, out decimal stok, out int? idJenis, out int? idMerek, out int? idSupplier))
            {
                return;
            }

            try
            {
                using SqliteConnection conn = _koneksi.GetConn();

                if (KodeSudahDipakaiNonaktif(conn, kode))
                {
                    DialogResult pilihan = MessageBox.Show(
                        "Kode barcode \"" + kode + "\" pernah dipakai barang yang "
                        + "sekarang dinonaktifkan.\n"
                        + "Aktifkan kembali barang ini dengan data yang baru?",
                        "Kode Sudah Dipakai",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (pilihan != DialogResult.Yes)
                    {
                        txtKode.Focus();
                        txtKode.SelectAll();
                        return;
                    }

                    AktifkanKembali(conn, kode, nama, satuan, hargaBeli, hargaJual, minGrosir, hargaGrosir, diskonPersen, stok, idJenis, idMerek, idSupplier);
                    TampilkanSukses("Barang \"" + nama + "\" berhasil diaktifkan kembali!");
                    BersihkanForm();
                    TampilData();
                    return;
                }

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
                    + "(kode_barcode, nama_barang, satuan, harga_beli, harga_jual, minimal_grosir, harga_grosir, diskon_persen, stok, id_jenis, id_merek, id_supplier, is_active) "
                    + "VALUES (@kode, @nama, @satuan, @hargaBeli, @hargaJual, @minGrosir, @hargaGrosir, @diskonPersen, @stok, @jenis, @merek, @supplier, 1)";

                using (SqliteCommand cmd = new(query, conn))
                {
                    cmd.Parameters.AddWithValue("@kode", kode);
                    cmd.Parameters.AddWithValue("@nama", nama);
                    cmd.Parameters.AddWithValue("@satuan", satuan);
                    cmd.Parameters.AddWithValue("@hargaBeli", hargaBeli);
                    cmd.Parameters.AddWithValue("@hargaJual", hargaJual);
                    cmd.Parameters.AddWithValue("@minGrosir", minGrosir);
                    cmd.Parameters.AddWithValue("@hargaGrosir", hargaGrosir);
                    cmd.Parameters.AddWithValue("@diskonPersen", diskonPersen);
                    cmd.Parameters.AddWithValue("@stok", stok);
                    cmd.Parameters.AddWithValue("@jenis", idJenis.HasValue ? idJenis.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@merek", idMerek.HasValue ? idMerek.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@supplier", idSupplier.HasValue ? idSupplier.Value : DBNull.Value);
                    cmd.ExecuteNonQuery();
                }

                CatatMutasiStok(conn, kode, "MASUK", stok, stok, "Barang baru", null);

                TampilkanSukses("Data barang berhasil ditambahkan!");
                BersihkanForm();
                TampilData();
            }
            catch (SqliteException ex) when (SqliteError.AdalahUnik(ex))
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

        private static void TampilkanSukses(string pesan)
        {
            MessageBox.Show(pesan, "Sukses",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private static bool KodeSudahAda(SqliteConnection conn, string kode)
        {
            const string query = "SELECT COUNT(*) FROM tb_barang WHERE kode_barcode = @kode";
            using SqliteCommand cmd = new(query, conn);
            cmd.Parameters.AddWithValue("@kode", kode);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        private static bool KodeSudahDipakaiNonaktif(SqliteConnection conn, string kode)
        {
            const string query = "SELECT COUNT(*) FROM tb_barang WHERE kode_barcode = @kode AND is_active = 0";
            using SqliteCommand cmd = new(query, conn);
            cmd.Parameters.AddWithValue("@kode", kode);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        private static void AktifkanKembali(
            SqliteConnection conn, string kode, string nama, string satuan,
            decimal hargaBeli, decimal hargaJual, decimal minGrosir, decimal hargaGrosir, decimal diskonPersen, decimal stok,
            int? idJenis, int? idMerek, int? idSupplier)
        {
            decimal stokLama = BacaStok(conn, kode);

            const string query =
                "UPDATE tb_barang SET nama_barang = @nama, satuan = @satuan, "
                + "harga_beli = @hargaBeli, harga_jual = @hargaJual, "
                + "minimal_grosir = @minGrosir, harga_grosir = @hargaGrosir, diskon_persen = @diskonPersen, "
                + "id_jenis = @jenis, id_merek = @merek, id_supplier = @supplier, "
                + "stok = @stok, is_active = 1 WHERE kode_barcode = @kode";

            using SqliteCommand cmd = new(query, conn);
            cmd.Parameters.AddWithValue("@nama", nama);
            cmd.Parameters.AddWithValue("@satuan", satuan);
            cmd.Parameters.AddWithValue("@hargaBeli", hargaBeli);
            cmd.Parameters.AddWithValue("@hargaJual", hargaJual);
            cmd.Parameters.AddWithValue("@minGrosir", minGrosir);
            cmd.Parameters.AddWithValue("@hargaGrosir", hargaGrosir);
            cmd.Parameters.AddWithValue("@diskonPersen", diskonPersen);
            cmd.Parameters.AddWithValue("@jenis", idJenis.HasValue ? idJenis.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@merek", idMerek.HasValue ? idMerek.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@supplier", idSupplier.HasValue ? idSupplier.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@stok", stok);
            cmd.Parameters.AddWithValue("@kode", kode);
            cmd.ExecuteNonQuery();

            decimal selisih = stok - stokLama;
            if (selisih != 0m)
            {
                CatatMutasiStok(conn, kode, "MASUK", selisih, stok,
                    "Barang diaktifkan kembali", null);
            }
        }

        private static void CatatMutasiStok(
            SqliteConnection conn, string kode, string tipe, decimal qty,
            decimal stokAkhir, string keterangan, SqliteTransaction? transaksi)
        {
            const string query =
                "INSERT INTO tb_mutasi_stok "
                + "(kode_barcode, tipe, qty, stok_akhir, keterangan, id_user) "
                + "VALUES (@kode, @tipe, @qty, @stokAkhir, @keterangan, @idUser)";

            using SqliteCommand cmd = new(query, conn, transaksi);
            cmd.Parameters.AddWithValue("@kode", kode);
            cmd.Parameters.AddWithValue("@tipe", tipe);
            cmd.Parameters.AddWithValue("@qty", qty);
            cmd.Parameters.AddWithValue("@stokAkhir", stokAkhir);
            cmd.Parameters.AddWithValue("@keterangan", keterangan);
            cmd.Parameters.AddWithValue("@idUser",
                Session.IsLoggedIn ? Session.UserId : (object)DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        // ------------------------------------------------------------------
        // Isi form dari tabel
        // ------------------------------------------------------------------
        private void dgvBarang_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow row = dgvBarang.Rows[e.RowIndex];
            if (row.DataBoundItem is not DataRowView data) return;

            txtKode.Text = Convert.ToString(data["Kode"]) ?? string.Empty;
            txtNama.Text = Convert.ToString(data["Nama Barang"]) ?? string.Empty;
            cmbSatuan.Text = Convert.ToString(data["Satuan"]) is { Length: > 0 } s ? s : SatuanBawaan;
            txtHargaBeli.Text = FormatNilaiRibuan(data["Harga Beli"]);
            txtHargaJual.Text = FormatNilaiRibuan(data["Harga Jual"]);
            txtMinGrosir.Text = FormatNilaiRibuan(data["Min. Grosir"]);
            txtHargaGrosir.Text = FormatNilaiRibuan(data["Harga Grosir"]);
            txtStok.Text = FormatNilaiRibuan(data["Stok"]);

            if (data.Row.Table.Columns.Contains("Diskon (%)"))
            {
                decimal dp = InputHelper.AmbilDecimal(data["Diskon (%)"]);
                txtDiskon.Text = dp > 0 ? dp.ToString("0.##") : string.Empty;
            }
            else
            {
                txtDiskon.Text = string.Empty;
            }

            // Set dropdown Jenis, Merek, Supplier
            int? idJ = data.Row.Table.Columns.Contains("id_jenis") ? InputHelper.AmbilInt(data["id_jenis"]) : (int?)null;
            int? idM = data.Row.Table.Columns.Contains("id_merek") ? InputHelper.AmbilInt(data["id_merek"]) : (int?)null;
            int? idS = data.Row.Table.Columns.Contains("id_supplier") ? InputHelper.AmbilInt(data["id_supplier"]) : (int?)null;

            PilihDropdownById(cmbJenis, idJ);
            PilihDropdownById(cmbMerek, idM);
            PilihDropdownById(cmbSupplier, idS);

            txtKode.Enabled = false;
            txtNama.Focus();
        }

        private static void PilihDropdownById(ComboBox cmb, int? id)
        {
            if (!id.HasValue || id.Value == 0)
            {
                if (cmb.Items.Count > 0) cmb.SelectedIndex = 0;
                return;
            }

            for (int i = 0; i < cmb.Items.Count; i++)
            {
                if (cmb.Items[i] is MasterComboItem item && item.Id == id.Value)
                {
                    cmb.SelectedIndex = i;
                    return;
                }
            }
            if (cmb.Items.Count > 0) cmb.SelectedIndex = 0;
        }

        private static string FormatNilaiRibuan(object? nilai)
        {
            if (nilai is null || nilai == DBNull.Value) return string.Empty;
            decimal d = InputHelper.AmbilDecimal(nilai);
            return d > 0 ? d.ToString("N0", new System.Globalization.CultureInfo("id-ID")) : string.Empty;
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

            if (!AmbilInput(out string kode, out string nama, out string satuan,
                    out decimal hargaBeli, out decimal hargaJual, out decimal minGrosir, out decimal hargaGrosir,
                    out decimal diskonPersen, out decimal stok, out int? idJenis, out int? idMerek, out int? idSupplier))
            {
                return;
            }

            try
            {
                using SqliteConnection conn = _koneksi.GetConn();

                decimal stokLama = BacaStok(conn, kode);
                if (stokLama < 0m)
                {
                    MessageBox.Show(
                        "Data \"" + kode + "\" tidak ditemukan. Silakan pilih ulang dari tabel.",
                        "Peringatan",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    txtKode.Enabled = true;
                    BersihkanForm();
                    TampilData();
                    return;
                }

                const string query =
                    "UPDATE tb_barang SET nama_barang = @nama, satuan = @satuan, "
                    + "harga_beli = @hargaBeli, harga_jual = @hargaJual, "
                    + "minimal_grosir = @minGrosir, harga_grosir = @hargaGrosir, diskon_persen = @diskonPersen, "
                    + "id_jenis = @jenis, id_merek = @merek, id_supplier = @supplier, "
                    + "stok = @stok WHERE kode_barcode = @kode";

                using (SqliteCommand cmd = new(query, conn))
                {
                    cmd.Parameters.AddWithValue("@nama", nama);
                    cmd.Parameters.AddWithValue("@satuan", satuan);
                    cmd.Parameters.AddWithValue("@hargaBeli", hargaBeli);
                    cmd.Parameters.AddWithValue("@hargaJual", hargaJual);
                    cmd.Parameters.AddWithValue("@minGrosir", minGrosir);
                    cmd.Parameters.AddWithValue("@hargaGrosir", hargaGrosir);
                    cmd.Parameters.AddWithValue("@diskonPersen", diskonPersen);
                    cmd.Parameters.AddWithValue("@jenis", idJenis.HasValue ? idJenis.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@merek", idMerek.HasValue ? idMerek.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@supplier", idSupplier.HasValue ? idSupplier.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@stok", stok);
                    cmd.Parameters.AddWithValue("@kode", kode);
                    cmd.ExecuteNonQuery();
                }

                decimal selisih = stok - stokLama;
                if (selisih != 0m)
                {
                    CatatMutasiStok(conn, kode, "PENYESUAIAN", selisih, stok,
                        "Koreksi stok oleh admin", null);
                }

                TampilkanSukses("Data barang berhasil diperbarui!");
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

        private static decimal BacaStok(SqliteConnection conn, string kode)
        {
            using SqliteCommand cmd = new("SELECT stok FROM tb_barang WHERE kode_barcode = @kode", conn);
            cmd.Parameters.AddWithValue("@kode", kode);
            object? hasil = cmd.ExecuteScalar();
            return hasil is null ? -1m : InputHelper.AmbilDecimal(hasil);
        }

        // ------------------------------------------------------------------
        // NONAKTIFKAN (soft delete)
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
                "Apakah Anda yakin ingin menonaktifkan barang \"" + txtNama.Text + "\"?\n\n"
                + "Barang tidak akan muncul lagi di daftar dan tidak bisa dijual,\n"
                + "tetapi seluruh riwayat transaksi lamanya tetap tersimpan.",
                "Konfirmasi Hapus",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (konfirmasi != DialogResult.Yes) return;

            try
            {
                using SqliteConnection conn = _koneksi.GetConn();
                const string query = "UPDATE tb_barang SET is_active = 0 WHERE kode_barcode = @kode";
                using SqliteCommand cmd = new(query, conn);
                cmd.Parameters.AddWithValue("@kode", txtKode.Text.Trim());
                cmd.ExecuteNonQuery();

                TampilkanSukses("Barang dinonaktifkan dan disembunyikan dari daftar.");
                txtKode.Enabled = true;
                BersihkanForm();
                TampilData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal menonaktifkan data: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnBersihkan_Click(object sender, EventArgs e)
        {
            BersihkanForm();
        }

        private void dgvBarang_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex < 0 || e.RowIndex < 0) return;

            string namaKolom = dgvBarang.Columns[e.ColumnIndex].HeaderText;

            if (namaKolom is "Harga Beli" or "Harga Jual" or "Harga Grosir")
            {
                decimal angka = InputHelper.AmbilDecimal(e.Value);
                e.Value = angka > 0 ? "Rp " + InputHelper.FormatNominal(angka) : "-";
                e.FormattingApplied = true;
            }
            else if (namaKolom is "Diskon (%)")
            {
                decimal angka = InputHelper.AmbilDecimal(e.Value);
                e.Value = angka > 0 ? angka.ToString("0.##") + "%" : "-";
                e.FormattingApplied = true;
            }
            else if (namaKolom is "Stok" or "Min. Grosir")
            {
                decimal angka = InputHelper.AmbilDecimal(e.Value);
                e.Value = angka > 0 ? InputHelper.FormatJumlah(angka) : "-";
                e.FormattingApplied = true;
            }
        }
    }
}
