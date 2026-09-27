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
            PasangMenuKlikKananKeranjang();
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
            decimal stokTersedia;

            try
            {
                using MySqlConnection conn = _koneksi.GetConn();
                conn.Open();

                // Barang non-aktif tidak boleh lagi dijual meskipun barcode-nya
                // masih tersimpan di database.
                const string query =
                    "SELECT nama_barang, harga_jual, stok FROM tb_barang "
                    + "WHERE kode_barcode = @kode AND is_active = 1 LIMIT 1";

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
                stokTersedia = InputHelper.AmbilDecimal(reader["stok"]);
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

            decimal qtyDiKeranjang = AmbilQtyKeranjang(kode);

            // Validasi stok: barang dengan stok 0 tidak boleh masuk keranjang,
            // dan total qty tidak boleh melebihi stok tersedia.
            if (stokTersedia <= 0m)
            {
                TampilkanPeringatan("Stok barang \"" + nama + "\" habis!");
                return;
            }

            if (qtyDiKeranjang + 1m > stokTersedia)
            {
                TampilkanPeringatan(
                    "Stok \"" + nama + "\" tidak cukup!\n"
                    + "Stok tersedia: " + InputHelper.FormatJumlah(stokTersedia)
                    + ", sudah di keranjang: " + InputHelper.FormatJumlah(qtyDiKeranjang) + ".");
                return;
            }

            if (qtyDiKeranjang > 0m)
            {
                // Barang sudah ada di keranjang -> tambahkan qty saja.
                PerbaruiBarisKeranjang(kode, qtyDiKeranjang + 1m);
            }
            else
            {
                dgvKeranjang.Rows.Add(kode, nama, harga, 1m, harga);
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
        private decimal AmbilQtyKeranjang(string kode)
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
                    return InputHelper.AmbilDecimal(row.Cells[3].Value);
                }
            }

            return 0m;
        }

        private void PerbaruiBarisKeranjang(string kode, decimal qtyBaru)
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
                row.Cells[4].Value = BulatkanSubtotal(harga * qtyBaru);
                dgvKeranjang.Refresh();
                return;
            }
        }

        /// <summary>
        /// Subtotal dibulatkan ke 2 desimal agar tampilan sama dengan nilai
        /// yang benar-benar disimpan di database (DECIMAL(15,2)).
        /// Tanpa ini, 3 x 333,33 bisa tampil 999,99 tetapi tersimpan 999,989.
        /// </summary>
        internal static decimal BulatkanSubtotal(decimal nilai)
        {
            return Math.Round(nilai, 2, MidpointRounding.AwayFromZero);
        }

        // =========================================================
        // 2. UBAH QTY / HAPUS ITEM
        // =========================================================

        /// <summary>
        /// Memasang menu klik kanan pada keranjang. Menu ini menggantikan
        /// kebutuhan memindai barcode berulang kali hanya untuk menambah qty.
        /// </summary>
        private void PasangMenuKlikKananKeranjang()
        {
            ContextMenuStrip menu = new();
            menu.Font = new Font("Segoe UI", 10F);

            ToolStripMenuItem itemUbahQty = new("Ubah Jumlah...");
            itemUbahQty.Click += (_, _) => UbahQtyBarisDipilih();

            ToolStripMenuItem itemHapus = new("Hapus Item");
            itemHapus.Click += (_, _) => HapusBarisDipilih();

            menu.Items.Add(itemUbahQty);
            menu.Items.Add(itemHapus);
            menu.Opening += (_, e) =>
            {
                bool adaBaris = AmbilBarisDipilih() is not null;
                itemUbahQty.Enabled = adaBaris;
                itemHapus.Enabled = adaBaris;
                e.Cancel = !adaBaris;
            };

            dgvKeranjang.ContextMenuStrip = menu;
        }

        /// <summary>
        /// Mengambil baris yang sedang dipilih, atau null bila tidak ada.
        /// </summary>
        private DataGridViewRow? AmbilBarisDipilih()
        {
            if (dgvKeranjang.CurrentCell is null)
            {
                return null;
            }

            DataGridViewRow row = dgvKeranjang.Rows[dgvKeranjang.CurrentCell.RowIndex];
            return row.IsNewRow ? null : row;
        }

        private void UbahQtyBarisDipilih()
        {
            DataGridViewRow? row = AmbilBarisDipilih();
            if (row is null)
            {
                return;
            }

            string kode = row.Cells[0].Value?.ToString() ?? string.Empty;
            string nama = row.Cells[1].Value?.ToString() ?? "(tanpa nama)";
            decimal qtyLama = InputHelper.AmbilDecimal(row.Cells[3].Value);

            // Stok terbaru dibaca dari database supaya angka yang dikoreksi
            // kasir tidak melebihi stok yang benar-benar ada.
            decimal stokTersedia;
            try
            {
                using MySqlConnection conn = _koneksi.GetConn();
                conn.Open();

                using MySqlCommand cmd = new(
                    "SELECT stok FROM tb_barang WHERE kode_barcode = @kode AND is_active = 1",
                    conn);
                cmd.Parameters.AddWithValue("@kode", kode);

                object? hasil = cmd.ExecuteScalar();
                if (hasil is null)
                {
                    TampilkanPeringatan("Barang \"" + nama + "\" tidak lagi bisa dijual.");
                    return;
                }

                stokTersedia = InputHelper.AmbilDecimal(hasil);
            }
            catch (Exception ex)
            {
                TampilkanPeringatan("Gagal membaca stok: " + ex.Message);
                return;
            }

            if (!InputDialog.Tanyakan(
                    "Ubah Jumlah",
                    "Jumlah \"" + nama + "\" (stok: " + InputHelper.FormatJumlah(stokTersedia) + ")",
                    InputHelper.FormatJumlah(qtyLama),
                    "Masukkan jumlah item (angka bulat, minimal 1). Maksimal: "
                        + InputHelper.FormatJumlah(stokTersedia),
                    out string masukan))
            {
                return;
            }

            // Validasi: Qty hanya boleh berupa bilangan bulat positif (minimal 1)
            string cleanMasukan = masukan.Trim().Replace(".", "").Replace(",", "");
            if (!int.TryParse(cleanMasukan, out int qtyInt) || qtyInt <= 0)
            {
                TampilkanPeringatan("Jumlah barang harus berupa bilangan bulat positif (minimal 1)!");
                return;
            }

            decimal qtyBaru = qtyInt;
            if (qtyBaru > stokTersedia)
            {
                TampilkanPeringatan(
                    "Jumlah melebihi stok tersedia.\n"
                    + "Stok \"" + nama + "\": " + InputHelper.FormatJumlah(stokTersedia));
                return;
            }

            PerbaruiBarisKeranjang(kode, qtyBaru);
            HitungTotalBelanja();
            txtBarcode.Focus();
        }

        private void HapusBarisDipilih()
        {
            DataGridViewRow? row = AmbilBarisDipilih();
            if (row is null)
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

            HapusBarisDipilih();
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

                // Subtotal dijumlahkan apa adanya karena sudah dibulatkan
                // oleh BulatkanSubtotal saat baris dibuat atau diperbarui.
                total += InputHelper.AmbilDecimal(row.Cells[4].Value);
            }

            _totalBelanja = total;
            lblTotal.Text = InputHelper.FormatNominal(total);

            // Hitung ulang kembalian agar tetap konsisten.
            PerbaruiKembalian();
        }

        private bool _isFormattingBayar;

        private void txtBayar_TextChanged(object sender, EventArgs e)
        {
            InputHelper.FormatRibuanOtomatis(txtBayar, ref _isFormattingBayar);
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
                string noNota = SimpanTransaksi(uangBayar, kembalian);

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
        ///
        /// Yang disimpan:
        ///   tb_transaksi           -> header nota + uang diterima &amp; kembalian
        ///   tb_detail_transaksi   -> satu baris per item, lengkap dengan snapshot
        ///                            nama_barang dan harga_satuan saat penjualan
        ///   tb_mutasi_stok        -> riwayat stok berkurang karena penjualan
        /// </summary>
        internal string SimpanTransaksi(decimal uangDiterima, decimal kembalian)
        {
            using MySqlConnection conn = _koneksi.GetConn();
            conn.Open();

            using MySqlTransaction transaksi = conn.BeginTransaction();

            try
            {
                // id_user diambil dari session, bukan angka hardcode.
                int idUser = Session.UserId;

                string noNota = BuatNomorNota();

                // uang_diterima dan kembalian disimpan agar laporan rekonsiliasi
                // kas bisa dilakukan belakangan (sebelumnya keduanya hilang).
                const string queryTransaksi =
                    "INSERT INTO tb_transaksi "
                    + "(no_nota, id_user, total_bayar, uang_diterima, kembalian) "
                    + "VALUES (@noNota, @idUser, @totalBayar, @uangDiterima, @kembalian)";

                long idTransaksi;
                using (MySqlCommand cmdTrans = new(queryTransaksi, conn, transaksi))
                {
                    cmdTrans.Parameters.AddWithValue("@noNota", noNota);
                    cmdTrans.Parameters.AddWithValue("@idUser", idUser);
                    cmdTrans.Parameters.AddWithValue("@totalBayar", _totalBelanja);
                    cmdTrans.Parameters.AddWithValue("@uangDiterima", uangDiterima);
                    cmdTrans.Parameters.AddWithValue("@kembalian", kembalian);
                    cmdTrans.ExecuteNonQuery();

                    // Kunci numerik hasil transaksi dipakai untuk mengaitkan detail.
                    idTransaksi = cmdTrans.LastInsertedId;
                }

                // nama_barang dan harga_satuan disimpan ulang sebagai snapshot.
                // Kalau harga atau nama barang diubah tujuh bulan kemudian, nota lama
                // tetap menampilkan informasi yang benar saat penjualan terjadi.
                const string queryDetail =
                    "INSERT INTO tb_detail_transaksi "
                    + "(id_transaksi, no_nota, kode_barcode, nama_barang, qty, "
                    + " harga_satuan, subtotal) "
                    + "VALUES (@idTransaksi, @noNota, @kode, @nama, @qty, @harga, @subtotal)";

                const string queryStok =
                    "UPDATE tb_barang SET stok = stok - @qty WHERE kode_barcode = @kode";

                const string queryCekStok =
                    "SELECT nama_barang, stok FROM tb_barang "
                    + "WHERE kode_barcode = @kode FOR UPDATE";

                // Setiap penjualan dicatat sebagai mutasi stok KELUAR (nilai negatif)
                // supaya bisa ditelusuri kenapa stok suatu barang berkurang.
                const string queryMutasi =
                    "INSERT INTO tb_mutasi_stok "
                    + "(kode_barcode, tipe, qty, stok_akhir, keterangan, id_user, ref_no_nota) "
                    + "VALUES (@kode, 'KELUAR', @qty, @stokAkhir, @keterangan, @idUser, @noNota)";

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

                    decimal qty = InputHelper.AmbilDecimal(row.Cells[3].Value);
                    if (qty <= 0m)
                    {
                        continue;
                    }

                    decimal hargaSatuan = InputHelper.AmbilDecimal(row.Cells[2].Value);
                    decimal subtotal = BulatkanSubtotal(InputHelper.AmbilDecimal(row.Cells[4].Value));

                    // Kunci baris barang dan cek stok terbaru.
                    // FOR UPDATE mencegah dua kasir menjual stok yang sama.
                    string namaBarang;
                    decimal stokTerbaru;

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
                        stokTerbaru = InputHelper.AmbilDecimal(reader["stok"]);
                    }

                    if (stokTerbaru < qty)
                    {
                        throw new StokTidakCukupException(
                            "Stok \"" + namaBarang + "\" tidak cukup!\n"
                            + "Diminta: " + InputHelper.FormatJumlah(qty)
                            + ", tersedia: " + InputHelper.FormatJumlah(stokTerbaru) + ".");
                    }

                    using (MySqlCommand cmdDetail = new(queryDetail, conn, transaksi))
                    {
                        cmdDetail.Parameters.AddWithValue("@idTransaksi", idTransaksi);
                        cmdDetail.Parameters.AddWithValue("@noNota", noNota);
                        cmdDetail.Parameters.AddWithValue("@kode", kodeBarang);
                        cmdDetail.Parameters.AddWithValue("@nama", namaBarang);
                        cmdDetail.Parameters.AddWithValue("@qty", qty);
                        cmdDetail.Parameters.AddWithValue("@harga", hargaSatuan);
                        cmdDetail.Parameters.AddWithValue("@subtotal", subtotal);
                        cmdDetail.ExecuteNonQuery();
                    }

                    using (MySqlCommand cmdStok = new(queryStok, conn, transaksi))
                    {
                        cmdStok.Parameters.AddWithValue("@qty", qty);
                        cmdStok.Parameters.AddWithValue("@kode", kodeBarang);
                        cmdStok.ExecuteNonQuery();
                    }

                    using (MySqlCommand cmdMutasi = new(queryMutasi, conn, transaksi))
                    {
                        cmdMutasi.Parameters.AddWithValue("@kode", kodeBarang);
                        cmdMutasi.Parameters.AddWithValue("@qty", -qty);
                        cmdMutasi.Parameters.AddWithValue("@stokAkhir", stokTerbaru - qty);
                        cmdMutasi.Parameters.AddWithValue(
                            "@keterangan", "Penualan " + noNota);
                        cmdMutasi.Parameters.AddWithValue("@idUser", idUser);
                        cmdMutasi.Parameters.AddWithValue("@noNota", noNota);
                        cmdMutasi.ExecuteNonQuery();
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
            if (e.ColumnIndex < 0 || e.RowIndex < 0)
            {
                return;
            }

            object? nilai = dgvKeranjang.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
            if (nilai is null)
            {
                return;
            }

            decimal angka = InputHelper.AmbilDecimal(nilai);

            // Kolom 2 dan 4 adalah uang (harga & subtotal), kolom 3 adalah jumlah.
            e.Value = e.ColumnIndex switch
            {
                2 or 4 => InputHelper.FormatNominal(angka),
                3 => InputHelper.FormatJumlah(angka),
                _ => e.Value,
            };

            e.FormattingApplied = e.ColumnIndex is 2 or 3 or 4;
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
