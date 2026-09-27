using System.Data;
using MySql.Data.MySqlClient;

namespace AplikasiKasirSMK4
{
    public partial class FormBarang : Form
    {
        private readonly Koneksi _koneksi = new();

        /// <summary>Nilai satuan untuk barang yang benar-benar baru dibuat.</summary>
        private const string SatuanBawaan = "pcs";

        /// <summary>
        /// Nilai maksimum yang aman untuk kolom DECIMAL(15,2).
        /// 15 total digit dengan 2 di antaranya untuk pecahan.
        /// </summary>
        private const decimal BatasStokMaksimum = 9999999999999.99m;

        private bool _isFormattingBeli;
        private bool _isFormattingJual;
        private bool _isFormattingStok;

        public FormBarang()
        {
            InitializeComponent();
            dgvBarang.CellFormatting += dgvBarang_CellFormatting;
            txtHargaBeli.TextChanged += txtHargaBeli_TextChanged;
            txtHargaJual.TextChanged += txtHargaJual_TextChanged;
            txtStok.TextChanged += txtStok_TextChanged;
        }

        private void txtHargaBeli_TextChanged(object? sender, EventArgs e)
        {
            InputHelper.FormatRibuanOtomatis(txtHargaBeli, ref _isFormattingBeli);
        }

        private void txtHargaJual_TextChanged(object? sender, EventArgs e)
        {
            InputHelper.FormatRibuanOtomatis(txtHargaJual, ref _isFormattingJual);
        }

        private void txtStok_TextChanged(object? sender, EventArgs e)
        {
            InputHelper.FormatRibuanOtomatis(txtStok, ref _isFormattingStok);
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

            cmbSatuan.Text = SatuanBawaan;
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

                // Barang yang sudah dinonaktifkan disembunyikan, bukan dihapus,
                // agar riwayat transaksi lama tetap utuh.
                const string query =
                    "SELECT kode_barcode AS 'Kode', nama_barang AS 'Nama Barang', "
                    + "satuan AS 'Satuan', harga_beli AS 'Harga Beli', "
                    + "harga_jual AS 'Harga Jual', stok AS 'Stok' "
                    + "FROM tb_barang WHERE is_active = 1 ORDER BY nama_barang ASC";

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
            cmbSatuan.Text = SatuanBawaan;
            txtHargaBeli.Text = string.Empty;
            txtHargaJual.Text = string.Empty;
            txtStok.Text = string.Empty;
            txtKode.Enabled = true;   // buka kembali kunci barcode
            txtKode.Focus();
        }

        private bool AmbilInput(out string kode, out string nama, out string satuan,
            out decimal hargaBeli, out decimal hargaJual, out decimal stok)
        {
            kode = txtKode.Text.Trim();
            nama = txtNama.Text.Trim();
            satuan = cmbSatuan.Text.Trim();
            if (satuan.Length == 0)
            {
                satuan = SatuanBawaan;
            }
            hargaBeli = 0m;
            hargaJual = 0m;
            stok = 0m;

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

            if (!InputHelper.TryParseNominal(txtStok.Text, out stok, out string pesanStok))
            {
                MessageBox.Show("Jumlah stok tidak valid: " + pesanStok + ".", "Peringatan",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtStok.Focus();
                return false;
            }

            if (stok < 0m)
            {
                MessageBox.Show("Jumlah stok tidak boleh negatif!", "Peringatan",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtStok.Focus();
                return false;
            }

            if (stok != Math.Floor(stok))
            {
                MessageBox.Show("Jumlah stok harus berupa bilangan bulat (tanpa pecahan/koma)!", "Peringatan",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtStok.Focus();
                return false;
            }

            if (stok > BatasStokMaksimum)
            {
                MessageBox.Show("Jumlah stok terlalu besar!", "Peringatan",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtStok.Focus();
                return false;
            }

            return true;
        }

        // ------------------------------------------------------------------
        // CREATE
        // ------------------------------------------------------------------
        private void btnSimpan_Click(object sender, EventArgs e)
        {
            if (!AmbilInput(out string kode, out string nama, out string satuan,
                    out decimal hargaBeli, out decimal hargaJual, out decimal stok))
            {
                return;
            }

            try
            {
                using MySqlConnection conn = _koneksi.GetConn();
                conn.Open();

                // Kode barcode pernah dipakai dan barangnya dinonaktifkan?
                // Kalau ya, tawarkan untuk mengaktifkan kembali daripada
                // gagal dengan pesan "kode sudah dipakai".
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

                    AktifkanKembali(conn, kode, nama, satuan, hargaBeli, hargaJual, stok);
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
                    + "(kode_barcode, nama_barang, satuan, harga_beli, harga_jual, stok, is_active) "
                    + "VALUES (@kode, @nama, @satuan, @hargaBeli, @hargaJual, @stok, 1)";

                using (MySqlCommand cmd = new(query, conn))
                {
                    cmd.Parameters.AddWithValue("@kode", kode);
                    cmd.Parameters.AddWithValue("@nama", nama);
                    cmd.Parameters.AddWithValue("@satuan", satuan);
                    cmd.Parameters.AddWithValue("@hargaBeli", hargaBeli);
                    cmd.Parameters.AddWithValue("@hargaJual", hargaJual);
                    cmd.Parameters.AddWithValue("@stok", stok);
                    cmd.ExecuteNonQuery();
                }

                // Barang baru = stok MASUK, dicatat di riwayat mutasi.
                CatatMutasiStok(conn, kode, "MASUK", stok, stok, "Barang baru", null);

                TampilkanSukses("Data barang berhasil ditambahkan!");

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

        private static void TampilkanSukses(string pesan)
        {
            MessageBox.Show(pesan, "Sukses",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private static bool KodeSudahAda(MySqlConnection conn, string kode)
        {
            const string query = "SELECT COUNT(*) FROM tb_barang WHERE kode_barcode = @kode";

            using MySqlCommand cmd = new(query, conn);
            cmd.Parameters.AddWithValue("@kode", kode);

            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        private static bool KodeSudahDipakaiNonaktif(MySqlConnection conn, string kode)
        {
            const string query =
                "SELECT COUNT(*) FROM tb_barang WHERE kode_barcode = @kode AND is_active = 0";

            using MySqlCommand cmd = new(query, conn);
            cmd.Parameters.AddWithValue("@kode", kode);

            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        private static void AktifkanKembali(
            MySqlConnection conn, string kode, string nama, string satuan,
            decimal hargaBeli, decimal hargaJual, decimal stok)
        {
            decimal stokLama = BacaStok(conn, kode);

            const string query =
                "UPDATE tb_barang SET nama_barang = @nama, satuan = @satuan, "
                + "harga_beli = @hargaBeli, harga_jual = @hargaJual, stok = @stok, "
                + "is_active = 1 WHERE kode_barcode = @kode";

            using MySqlCommand cmd = new(query, conn);
            cmd.Parameters.AddWithValue("@nama", nama);
            cmd.Parameters.AddWithValue("@satuan", satuan);
            cmd.Parameters.AddWithValue("@hargaBeli", hargaBeli);
            cmd.Parameters.AddWithValue("@hargaJual", hargaJual);
            cmd.Parameters.AddWithValue("@stok", stok);
            cmd.Parameters.AddWithValue("@kode", kode);
            cmd.ExecuteNonQuery();

            // Yang dicatat adalah selisihnya, bukan seluruh stok. Kalau stok
            // lama 99 lalu diisi 50, riwayatnya harus menunjukkan -49.
            decimal selisih = stok - stokLama;
            if (selisih != 0m)
            {
                CatatMutasiStok(conn, kode, "MASUK", selisih, stok,
                    "Barang diaktifkan kembali", null);
            }
        }

        /// <summary>
        /// Membaca satuan barang. Mengembalikan <see cref="SatuanBawaan"/> bila
        /// barcode tidak ditemukan atau satuan kosong.
        /// </summary>
        private static string BacaSatuan(MySqlConnection conn, string kode)
        {
            using MySqlCommand cmd = new(
                "SELECT satuan FROM tb_barang WHERE kode_barcode = @kode", conn);
            cmd.Parameters.AddWithValue("@kode", kode);

            return Convert.ToString(cmd.ExecuteScalar()) is { Length: > 0 } s
                ? s
                : SatuanBawaan;
        }

        /// <summary>
        /// Mencatat satu perubahan stok ke tb_mutasi_stok supaya bisa ditelusuri
        /// kenapa stok suatu barang berubah. qty positif berarti stok bertambah.
        /// </summary>
        private static void CatatMutasiStok(
            MySqlConnection conn, string kode, string tipe, decimal qty,
            decimal stokAkhir, string keterangan, MySqlTransaction? transaksi)
        {
            const string query =
                "INSERT INTO tb_mutasi_stok "
                + "(kode_barcode, tipe, qty, stok_akhir, keterangan, id_user) "
                + "VALUES (@kode, @tipe, @qty, @stokAkhir, @keterangan, @idUser)";

            using MySqlCommand cmd = new(query, conn, transaksi);
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
            cmbSatuan.Text = Convert.ToString(data["Satuan"]) is { Length: > 0 } s ? s : SatuanBawaan;
            txtHargaBeli.Text = FormatNilaiRibuan(data["Harga Beli"]);
            txtHargaJual.Text = FormatNilaiRibuan(data["Harga Jual"]);
            txtStok.Text = FormatNilaiRibuan(data["Stok"]);

            // Kode barcode dikunci saat mode edit.
            txtKode.Enabled = false;
            txtNama.Focus();
        }

        private static string FormatNilaiRibuan(object? nilai)
        {
            if (nilai is null || nilai == DBNull.Value) return string.Empty;
            decimal d = InputHelper.AmbilDecimal(nilai);
            return d.ToString("N0", new System.Globalization.CultureInfo("id-ID"));
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
                    out decimal hargaBeli, out decimal hargaJual, out decimal stok))
            {
                return;
            }

            try
            {
                using MySqlConnection conn = _koneksi.GetConn();
                conn.Open();

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

                // Stok boleh naik-turun, jadi selisihnya dicatat sebagai
                // PENYESUAIAN (hasil opname, barang rusak, atau tak laku).
                const string query =
                    "UPDATE tb_barang SET nama_barang = @nama, satuan = @satuan, "
                    + "harga_beli = @hargaBeli, harga_jual = @hargaJual, stok = @stok "
                    + "WHERE kode_barcode = @kode";

                using (MySqlCommand cmd = new(query, conn))
                {
                    cmd.Parameters.AddWithValue("@nama", nama);
                    cmd.Parameters.AddWithValue("@satuan", satuan);
                    cmd.Parameters.AddWithValue("@hargaBeli", hargaBeli);
                    cmd.Parameters.AddWithValue("@hargaJual", hargaJual);
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

        /// <summary>
        /// Membaca stok terkini. Mengembalikan -1 bila barcode tidak ditemukan,
        /// karena nilai stok di database tidak mungkin negatif (ada CHECK).
        /// </summary>
        private static decimal BacaStok(MySqlConnection conn, string kode)
        {
            using MySqlCommand cmd = new(
                "SELECT stok FROM tb_barang WHERE kode_barcode = @kode", conn);
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
                + "tetapi seluruh riwayat transaksi lamanya tetap tersimpan.\n"
                + "Barang bisa diaktifkan kembali kapan saja dengan menekan SIMPAN\n"
                + "lalu mengisi kode barcode yang sama.",
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

                // Delete logis, bukan DELETE FROM. Ini wajib karena tb_detail_transaksi
                // punya foreign key ke tb_barang dengan ON DELETE RESTRICT.
                const string query =
                    "UPDATE tb_barang SET is_active = 0 WHERE kode_barcode = @kode";

                using MySqlCommand cmd = new(query, conn);
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

            string namaKolom = dgvBarang.Columns[e.ColumnIndex].Name;

            // Kolom harga berisi rupiah, kolom stok berisi jumlah barang.
            if (namaKolom is not ("Harga Beli" or "Harga Jual" or "Stok"))
            {
                return;
            }

            object? nilai = dgvBarang.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
            if (nilai is null)
            {
                return;
            }

            decimal angka = InputHelper.AmbilDecimal(nilai);

            e.Value = namaKolom == "Stok"
                ? InputHelper.FormatJumlah(angka)
                : InputHelper.FormatNominal(angka);
            e.FormattingApplied = true;
        }
    }
}
