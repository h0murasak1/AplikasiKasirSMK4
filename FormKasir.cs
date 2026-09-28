using System.ComponentModel;
using System.Data;
using Microsoft.Data.Sqlite;

namespace AplikasiKasirSMK4
{
    public partial class FormKasir : Form
    {
        private readonly Koneksi _koneksi = new();
        private decimal _totalBelanjaKotor;
        private decimal _totalBelanja;
        private decimal _diskonMemberPersen;
        private decimal _diskonMemberNominal;
        private decimal _diskonManualNominal;
        private decimal _diskonTotalNominal;
        private bool _sedangKeluar;

        // Kontrol tambahan untuk Member, Sales, Metode Bayar, Diskon Tambahan, Header & Logout
        private readonly ComboBox cmbMember = new();
        private readonly ComboBox cmbSales = new();
        private readonly ComboBox cmbMetodeBayar = new();
        private readonly TextBox txtDiskonManual = new();
        private readonly Label lblSubtotalInfo = new();
        private readonly Label lblMemberPoinInfo = new();
        private readonly Label lblUserInfo = new();
        private readonly Button btnLogoutKasir = new();
        private readonly Button btnSettingPrinter = new();

        private sealed class ItemPilihan
        {
            public int? Id { get; }
            public string Teks { get; }
            public decimal Tag { get; }
            public string Jenis { get; }

            public ItemPilihan(int? id, string teks, decimal tag = 0m, string jenis = "")
            {
                Id = id;
                Teks = teks;
                Tag = tag;
                Jenis = jenis;
            }

            public override string ToString() => Teks;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsRootForm { get; set; }

        public FormKasir()
        {
            InitializeComponent();
            UiThemeHelper.TerapkanIkon(this);
            UiThemeHelper.FormatTabel(dgvKeranjang);
            dgvKeranjang.RowTemplate.Height = 36;
            dgvKeranjang.DefaultCellStyle.Font = new Font("Segoe UI", 10F);
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

            TataUlangHeader();
            TataUlangPanelKanan();
            MuatPilihanKasir();
            ResetTransaksi();
            txtBarcode.Focus();
        }

        private void TataUlangHeader()
        {
            if (!IsRootForm)
            {
                panelHeader.Visible = false;
                return;
            }

            panelHeader.Visible = true;
            panelHeader.Controls.Clear();
            panelHeader.Height = 70;

            lblJudul.Location = new Point(16, 18);
            lblJudul.AutoSize = true;
            lblJudul.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            lblJudul.ForeColor = Color.White;
            lblJudul.Text = "SISTEM POS SMK NEGERI 4";
            panelHeader.Controls.Add(lblJudul);

            // Panel sisi kanan header
            var pnlKananHeader = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                Padding = new Padding(0, 16, 16, 0),
                BackColor = Color.Transparent
            };

            lblUserInfo.Text = $"👤 {Session.NamaLengkap} ({Session.Role})";
            lblUserInfo.ForeColor = Color.White;
            lblUserInfo.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblUserInfo.AutoSize = true;
            lblUserInfo.Margin = new Padding(0, 8, 12, 0);

            btnSettingPrinter.Text = "🖨️ Printer & Drawer";
            btnSettingPrinter.Height = 36;
            btnSettingPrinter.Width = 145;
            btnSettingPrinter.FlatStyle = FlatStyle.Flat;
            btnSettingPrinter.BackColor = Color.FromArgb(41, 128, 185);
            btnSettingPrinter.ForeColor = Color.White;
            btnSettingPrinter.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            btnSettingPrinter.Cursor = Cursors.Hand;
            btnSettingPrinter.FlatAppearance.BorderColor = Color.White;
            btnSettingPrinter.FlatAppearance.BorderSize = 1;
            btnSettingPrinter.Margin = new Padding(0, 0, 8, 0);
            btnSettingPrinter.Click += (_, _) =>
            {
                using var dlg = new FormPengaturanPrinter();
                dlg.ShowDialog(this);
            };

            btnLogoutKasir.Text = IsRootForm ? "🚪 LOGOUT" : "🚪 KEMBALI";
            btnLogoutKasir.Height = 36;
            btnLogoutKasir.Width = 110;
            btnLogoutKasir.FlatStyle = FlatStyle.Flat;
            btnLogoutKasir.BackColor = Color.FromArgb(231, 76, 60);
            btnLogoutKasir.ForeColor = Color.White;
            btnLogoutKasir.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnLogoutKasir.Cursor = Cursors.Hand;
            btnLogoutKasir.FlatAppearance.BorderSize = 0;
            btnLogoutKasir.Click += BtnLogoutKasir_Click;

            pnlKananHeader.Controls.Add(lblUserInfo);
            pnlKananHeader.Controls.Add(btnSettingPrinter);
            pnlKananHeader.Controls.Add(btnLogoutKasir);

            panelHeader.Controls.Add(pnlKananHeader);
        }

        private void BtnLogoutKasir_Click(object? sender, EventArgs e)
        {
            string pesan = IsRootForm 
                ? "Anda yakin ingin logout dari akun kasir?" 
                : "Tutup halaman kasir dan kembali ke menu utama?";

            DialogResult konfirmasi = MessageBox.Show(
                pesan,
                "Konfirmasi",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (konfirmasi != DialogResult.Yes) return;

            _sedangKeluar = true;

            if (IsRootForm)
            {
                Session.Clear();
                new Form1().Show();
            }
            Close();
        }

        // =========================================================
        // 0. TATA LETAK & PILIHAN KASIR (MEMBER, SALES, METODE BAYAR)
        // =========================================================

        private void TataUlangPanelKanan()
        {
            panelKanan.Controls.Clear();
            panelKanan.Width = 380;
            panelKanan.AutoScroll = true;

            int x = 16;
            int lebar = 330;
            int y = 10;

            // Total Belanja Besar
            lblTotalTitle.Location = new Point(x, y);
            lblTotalTitle.Size = new Size(lebar, 20);
            lblTotalTitle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            panelKanan.Controls.Add(lblTotalTitle);
            y += 24;

            lblTotal.Location = new Point(x, y);
            lblTotal.Size = new Size(lebar, 60);
            lblTotal.Font = new Font("Consolas", 26F, FontStyle.Bold);
            panelKanan.Controls.Add(lblTotal);
            y += 66;

            // Subtotal & Diskon Info
            lblSubtotalInfo.Location = new Point(x, y);
            lblSubtotalInfo.Size = new Size(lebar, 20);
            lblSubtotalInfo.Font = new Font("Segoe UI", 8.5F);
            lblSubtotalInfo.ForeColor = Color.FromArgb(80, 80, 80);
            lblSubtotalInfo.Text = "Subtotal: Rp 0";
            panelKanan.Controls.Add(lblSubtotalInfo);
            y += 26;

            // Member
            var lblMember = new Label
            {
                Text = "Member Pelanggan:",
                Location = new Point(x, y),
                Size = new Size(lebar, 18),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            panelKanan.Controls.Add(lblMember);
            y += 20;

            cmbMember.Location = new Point(x, y);
            cmbMember.Size = new Size(lebar, 28);
            cmbMember.Font = new Font("Segoe UI", 9.5F);
            cmbMember.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMember.SelectedIndexChanged += cmbMember_SelectedIndexChanged;
            panelKanan.Controls.Add(cmbMember);
            y += 32;

            lblMemberPoinInfo.Location = new Point(x, y);
            lblMemberPoinInfo.Size = new Size(lebar, 16);
            lblMemberPoinInfo.Font = new Font("Segoe UI", 8F);
            lblMemberPoinInfo.ForeColor = Color.FromArgb(39, 174, 96);
            panelKanan.Controls.Add(lblMemberPoinInfo);
            y += 20;

            // Sales
            var lblSales = new Label
            {
                Text = "Sales (Opsional):",
                Location = new Point(x, y),
                Size = new Size(lebar, 18),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            panelKanan.Controls.Add(lblSales);
            y += 20;

            cmbSales.Location = new Point(x, y);
            cmbSales.Size = new Size(lebar, 28);
            cmbSales.Font = new Font("Segoe UI", 9.5F);
            cmbSales.DropDownStyle = ComboBoxStyle.DropDownList;
            panelKanan.Controls.Add(cmbSales);
            y += 34;

            // Metode Bayar
            var lblMetode = new Label
            {
                Text = "Metode Pembayaran:",
                Location = new Point(x, y),
                Size = new Size(lebar, 18),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            panelKanan.Controls.Add(lblMetode);
            y += 20;

            cmbMetodeBayar.Location = new Point(x, y);
            cmbMetodeBayar.Size = new Size(lebar, 28);
            cmbMetodeBayar.Font = new Font("Segoe UI", 9.5F);
            cmbMetodeBayar.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMetodeBayar.SelectedIndexChanged += cmbMetodeBayar_SelectedIndexChanged;
            panelKanan.Controls.Add(cmbMetodeBayar);
            y += 36;

            // Diskon Tambahan (Manual)
            var lblDiskon = new Label
            {
                Text = "Diskon Tambahan (Rp / Opsional):",
                Location = new Point(x, y),
                Size = new Size(lebar, 18),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            panelKanan.Controls.Add(lblDiskon);
            y += 20;

            txtDiskonManual.Location = new Point(x, y);
            txtDiskonManual.Size = new Size(lebar, 26);
            txtDiskonManual.Font = new Font("Segoe UI", 10F);
            txtDiskonManual.TextChanged += (_, _) => HitungTotalBelanja();
            panelKanan.Controls.Add(txtDiskonManual);
            y += 34;

            // Uang Bayar
            lblBayarTitle.Location = new Point(x, y);
            lblBayarTitle.Size = new Size(lebar, 20);
            lblBayarTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            panelKanan.Controls.Add(lblBayarTitle);
            y += 24;

            txtBayar.Location = new Point(x, y);
            txtBayar.Size = new Size(lebar, 48);
            panelKanan.Controls.Add(txtBayar);
            y += 56;

            // Tombol pecahan uang bayar (5.000 s/d 200.000)
            var flowPecahan = new FlowLayoutPanel
            {
                Location = new Point(x, y),
                Size = new Size(lebar, 64),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoSize = false,
                Margin = Padding.Empty,
                Padding = Padding.Empty,
            };

            var pecahan = new (string Label, decimal Nilai)[]
            {
                ("5.000", 5_000m),
                ("10.000", 10_000m),
                ("20.000", 20_000m),
                ("50.000", 50_000m),
                ("100.000", 100_000m),
                ("200.000", 200_000m),
            };

            int btnW = (lebar - 8) / 3;
            int btnH = 28;
            foreach (var (label, nilai) in pecahan)
            {
                var btn = new Button
                {
                    Text = label,
                    Width = btnW,
                    Height = btnH,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    Margin = new Padding(1),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(41, 128, 185),
                    ForeColor = Color.White,
                    Cursor = Cursors.Hand,
                };
                btn.FlatAppearance.BorderSize = 0;
                decimal nilaiTombol = nilai;
                btn.Click += (_, _) =>
                {
                    txtBayar.Text = InputHelper.FormatNominal(nilaiTombol);
                };
                flowPecahan.Controls.Add(btn);
            }

            panelKanan.Controls.Add(flowPecahan);
            y += 70;

            // Kembalian
            lblKembalianTitle.Location = new Point(x, y);
            lblKembalianTitle.Size = new Size(lebar, 20);
            lblKembalianTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            panelKanan.Controls.Add(lblKembalianTitle);
            y += 24;

            lblKembalian.Location = new Point(x, y);
            lblKembalian.Size = new Size(lebar, 46);
            lblKembalian.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            panelKanan.Controls.Add(lblKembalian);
            y += 52;

            // Tombol Bayar
            btnBayar.Location = new Point(x, y);
            btnBayar.Size = new Size(lebar, 54);
            btnBayar.Dock = DockStyle.None;
            panelKanan.Controls.Add(btnBayar);
            y += 64;
        }

        private void MuatPilihanKasir()
        {
            // 1. Muat Member
            cmbMember.Items.Clear();
            cmbMember.Items.Add(new ItemPilihan(null, "-- Tanpa Member (Umum) --", 0m));

            try
            {
                using SqliteConnection conn = _koneksi.GetConn();

                using (SqliteCommand cmd = new(
                    "SELECT id_member, kode_member, nama_member, diskon_persen FROM tb_member "
                    + "WHERE is_active = 1 ORDER BY nama_member", conn))
                using (SqliteDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        int id = InputHelper.AmbilInt(r["id_member"]);
                        string kode = r["kode_member"]?.ToString() ?? "";
                        string nama = r["nama_member"]?.ToString() ?? "";
                        decimal dis = InputHelper.AmbilDecimal(r["diskon_persen"]);
                        string teks = $"{kode} - {nama}" + (dis > 0 ? $" (Diskon {dis:0.##}%)" : "");
                        cmbMember.Items.Add(new ItemPilihan(id, teks, dis));
                    }
                }

                // 2. Muat Sales
                cmbSales.Items.Clear();
                cmbSales.Items.Add(new ItemPilihan(null, "-- Tanpa Sales --", 0m));
                using (SqliteCommand cmd = new(
                    "SELECT id_sales, kode_sales, nama_sales, komisi_persen FROM tb_sales "
                    + "WHERE is_active = 1 ORDER BY nama_sales", conn))
                using (SqliteDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        int id = InputHelper.AmbilInt(r["id_sales"]);
                        string kode = r["kode_sales"]?.ToString() ?? "";
                        string nama = r["nama_sales"]?.ToString() ?? "";
                        decimal kom = InputHelper.AmbilDecimal(r["komisi_persen"]);
                        string teks = (kode.Length > 0 ? $"{kode} - " : "") + nama + (kom > 0 ? $" ({kom:0.##}%)" : "");
                        cmbSales.Items.Add(new ItemPilihan(id, teks, kom));
                    }
                }

                // 3. Muat Metode Bayar (Non-tunai diberi label Under Development)
                cmbMetodeBayar.Items.Clear();
                using (SqliteCommand cmd = new(
                    "SELECT id_metode, nama_metode, jenis FROM tb_metode_bayar "
                    + "WHERE is_aktif = 1 ORDER BY urutan, id_metode", conn))
                using (SqliteDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        int id = InputHelper.AmbilInt(r["id_metode"]);
                        string nama = r["nama_metode"]?.ToString() ?? "";
                        string jenis = r["jenis"]?.ToString() ?? "TUNAI";
                        string teks = jenis == "TUNAI" ? nama : $"{nama} (Under Development)";
                        cmbMetodeBayar.Items.Add(new ItemPilihan(id, teks, 0m, jenis));
                    }
                }

                if (cmbMetodeBayar.Items.Count == 0)
                {
                    // Sisipkan metode bawaan jika belum ada
                    using (SqliteCommand cmdInit = new(
                        "INSERT OR IGNORE INTO tb_metode_bayar (nama_metode, jenis, urutan, is_aktif) VALUES "
                        + "('Tunai', 'TUNAI', 1, 1), "
                        + "('Debit / Kredit', 'NON_TUNAI', 2, 1), "
                        + "('QRIS', 'NON_TUNAI', 3, 1), "
                        + "('Transfer Bank', 'NON_TUNAI', 4, 1), "
                        + "('Tempo (Piutang)', 'TEMPO', 5, 1)", conn))
                    {
                        cmdInit.ExecuteNonQuery();
                    }

                    using (SqliteCommand cmd = new(
                        "SELECT id_metode, nama_metode, jenis FROM tb_metode_bayar "
                        + "WHERE is_aktif = 1 ORDER BY urutan, id_metode", conn))
                    using (SqliteDataReader r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            int id = InputHelper.AmbilInt(r["id_metode"]);
                            string nama = r["nama_metode"]?.ToString() ?? "";
                            string jenis = r["jenis"]?.ToString() ?? "TUNAI";
                            string teks = jenis == "TUNAI" ? nama : $"{nama} (Under Development)";
                            cmbMetodeBayar.Items.Add(new ItemPilihan(id, teks, 0m, jenis));
                        }
                    }
                }
            }
            catch { /* biarkan default */ }

            if (cmbMember.Items.Count > 0) cmbMember.SelectedIndex = 0;
            if (cmbSales.Items.Count > 0) cmbSales.SelectedIndex = 0;
            if (cmbMetodeBayar.Items.Count > 0) cmbMetodeBayar.SelectedIndex = 0;
        }

        private void cmbMember_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cmbMember.SelectedItem is ItemPilihan item && item.Id.HasValue)
            {
                _diskonMemberPersen = item.Tag;
                lblMemberPoinInfo.Text = item.Tag > 0 ? $"Diskon member aktif: {item.Tag:0.##}%" : "Member aktif";
            }
            else
            {
                _diskonMemberPersen = 0m;
                lblMemberPoinInfo.Text = "";
            }

            HitungTotalBelanja();
        }

        private void cmbMetodeBayar_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cmbMetodeBayar.SelectedItem is not ItemPilihan item) return;

            if (item.Jenis != "TUNAI")
            {
                MessageBox.Show(
                    $"Metode pembayaran \"{item.Teks}\" saat ini masih dalam tahap pengembangan (Under Development) dan belum dapat digunakan.\nSilakan gunakan metode Tunai.",
                    "Under Development",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                cmbMetodeBayar.SelectedIndex = 0;
                return;
            }

            txtBayar.Enabled = true;
            PerbaruiKembalian();
        }

        // =========================================================
        // 1. SCAN BARCODE & PERHITUNGAN GROSIR
        // =========================================================
        private void txtBarcode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;

            e.SuppressKeyPress = true;

            string kode = txtBarcode.Text.Trim();
            if (kode.Length == 0) return;

            CariDanMasukKeranjang(kode);
        }

        private void CariDanMasukKeranjang(string kode)
        {
            string nama;
            decimal hargaEcer;
            decimal minGrosir;
            decimal hargaGrosir;
            decimal stokTersedia;
            decimal diskonPersen;

            try
            {
                using SqliteConnection conn = _koneksi.GetConn();

                const string query =
                    "SELECT nama_barang, harga_jual, stok, "
                    + "COALESCE(minimal_grosir, 0) AS minimal_grosir, "
                    + "COALESCE(harga_grosir, 0) AS harga_grosir, "
                    + "COALESCE(diskon_persen, 0) AS diskon_persen "
                    + "FROM tb_barang WHERE kode_barcode = @kode AND is_active = 1 LIMIT 1";

                using SqliteCommand cmd = new(query, conn);
                cmd.Parameters.AddWithValue("@kode", kode);

                using SqliteDataReader reader = cmd.ExecuteReader();
                if (!reader.Read())
                {
                    TampilkanPeringatanBarang(kode);
                    return;
                }

                nama = reader["nama_barang"]?.ToString() ?? string.Empty;
                hargaEcer = InputHelper.AmbilDecimal(reader["harga_jual"]);
                stokTersedia = InputHelper.AmbilDecimal(reader["stok"]);
                minGrosir = InputHelper.AmbilDecimal(reader["minimal_grosir"]);
                hargaGrosir = InputHelper.AmbilDecimal(reader["harga_grosir"]);
                diskonPersen = InputHelper.AmbilDecimal(reader["diskon_persen"]);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal mencari barang: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (hargaEcer <= 0m)
            {
                TampilkanPeringatan("Barang \"" + nama + "\" tidak memiliki harga jual yang valid.");
                return;
            }

            decimal qtyDiKeranjang = AmbilQtyKeranjang(kode);

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

            decimal qtyBaru = qtyDiKeranjang + 1m;
            decimal hargaEfektif = (minGrosir > 0 && hargaGrosir > 0 && qtyBaru >= minGrosir)
                ? hargaGrosir
                : hargaEcer;

            // Harga setelah diskon per barang
            decimal hargaSetelahDiskon = diskonPersen > 0m
                ? BulatkanSubtotal(hargaEfektif * (1m - diskonPersen / 100m))
                : hargaEfektif;

            if (qtyDiKeranjang > 0m)
            {
                PerbaruiBarisKeranjang(kode, qtyBaru, hargaEfektif);
            }
            else
            {
                // Kode, Nama, Harga, Diskon(%), Qty, Subtotal
                dgvKeranjang.Rows.Add(kode, nama, hargaEfektif, diskonPersen, 1m, hargaSetelahDiskon);
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

        private decimal AmbilQtyKeranjang(string kode)
        {
            foreach (DataGridViewRow row in dgvKeranjang.Rows)
            {
                if (row.IsNewRow) continue;

                if (string.Equals(row.Cells[0].Value?.ToString(), kode,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return InputHelper.AmbilDecimal(row.Cells[4].Value);
                }
            }

            return 0m;
        }

        private void PerbaruiBarisKeranjang(string kode, decimal qtyBaru, decimal? hargaBaru = null)
        {
            foreach (DataGridViewRow row in dgvKeranjang.Rows)
            {
                if (row.IsNewRow) continue;

                if (!string.Equals(row.Cells[0].Value?.ToString(), kode,
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                decimal harga = hargaBaru ?? InputHelper.AmbilDecimal(row.Cells[2].Value);
                decimal diskon = InputHelper.AmbilDecimal(row.Cells[3].Value);
                decimal hargaSetelahDiskon = diskon > 0m ? BulatkanSubtotal(harga * (1m - diskon / 100m)) : harga;
                row.Cells[2].Value = harga;
                row.Cells[4].Value = qtyBaru;
                row.Cells[5].Value = BulatkanSubtotal(hargaSetelahDiskon * qtyBaru);
                dgvKeranjang.Refresh();
                return;
            }
        }

        internal static decimal BulatkanSubtotal(decimal nilai)
        {
            return Math.Round(nilai, 2, MidpointRounding.AwayFromZero);
        }

        // =========================================================
        // 2. UBAH QTY / HAPUS ITEM (DENGAN HITUNGAN GROSIR OTOMATIS)
        // =========================================================

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

        private DataGridViewRow? AmbilBarisDipilih()
        {
            if (dgvKeranjang.CurrentCell is null) return null;
            DataGridViewRow row = dgvKeranjang.Rows[dgvKeranjang.CurrentCell.RowIndex];
            return row.IsNewRow ? null : row;
        }

        private void UbahQtyBarisDipilih()
        {
            DataGridViewRow? row = AmbilBarisDipilih();
            if (row is null) return;

            string kode = row.Cells[0].Value?.ToString() ?? string.Empty;
            string nama = row.Cells[1].Value?.ToString() ?? "(tanpa nama)";
            decimal qtyLama = InputHelper.AmbilDecimal(row.Cells[4].Value);

            decimal stokTersedia;
            decimal hargaEcer;
            decimal minGrosir;
            decimal hargaGrosir;

            try
            {
                using SqliteConnection conn = _koneksi.GetConn();
                using SqliteCommand cmd = new(
                    "SELECT stok, harga_jual, COALESCE(minimal_grosir, 0) AS minimal_grosir, "
                    + "COALESCE(harga_grosir, 0) AS harga_grosir "
                    + "FROM tb_barang WHERE kode_barcode = @kode AND is_active = 1", conn);
                cmd.Parameters.AddWithValue("@kode", kode);

                using SqliteDataReader r = cmd.ExecuteReader();
                if (!r.Read())
                {
                    TampilkanPeringatan("Barang \"" + nama + "\" tidak lagi bisa dijual.");
                    return;
                }

                stokTersedia = InputHelper.AmbilDecimal(r["stok"]);
                hargaEcer = InputHelper.AmbilDecimal(r["harga_jual"]);
                minGrosir = InputHelper.AmbilDecimal(r["minimal_grosir"]);
                hargaGrosir = InputHelper.AmbilDecimal(r["harga_grosir"]);
            }
            catch (Exception ex)
            {
                TampilkanPeringatan("Gagal membaca stok: " + ex.Message);
                return;
            }

            string infoGrosir = (minGrosir > 0 && hargaGrosir > 0)
                ? $"\n(Grosir: Min {minGrosir:0.##} pcs = Rp {InputHelper.FormatNominal(hargaGrosir)})"
                : "";

            if (!InputDialog.Tanyakan(
                    "Ubah Jumlah",
                    "Jumlah \"" + nama + "\" (stok: " + InputHelper.FormatJumlah(stokTersedia) + ")" + infoGrosir,
                    InputHelper.FormatJumlah(qtyLama),
                    "Masukkan jumlah item (angka bulat, minimal 1). Maksimal: "
                        + InputHelper.FormatJumlah(stokTersedia),
                    out string masukan))
            {
                return;
            }

            if (!InputHelper.TryParseBilanBulat(masukan, out int qtyInt, out string pesanQty)
                || qtyInt < 1)
            {
                TampilkanPeringatan(
                    "Jumlah barang harus berupa bilangan bulat positif (minimal 1)!"
                    + Environment.NewLine + pesanQty);
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

            decimal hargaEfektif = (minGrosir > 0 && hargaGrosir > 0 && qtyBaru >= minGrosir)
                ? hargaGrosir
                : hargaEcer;

            PerbaruiBarisKeranjang(kode, qtyBaru, hargaEfektif);
            HitungTotalBelanja();
            txtBarcode.Focus();
        }

        private void HapusBarisDipilih()
        {
            DataGridViewRow? row = AmbilBarisDipilih();
            if (row is null) return;

            string nama = row.Cells[1].Value?.ToString() ?? "(tanpa nama)";

            DialogResult konfirmasi = MessageBox.Show(
                "Hapus \"" + nama + "\" dari keranjang?",
                "Konfirmasi Hapus Item",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (konfirmasi != DialogResult.Yes) return;

            dgvKeranjang.Rows.Remove(row);
            HitungTotalBelanja();
            txtBarcode.Focus();
        }

        private void dgvKeranjang_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            DataGridViewRow row = dgvKeranjang.Rows[e.RowIndex];
            if (row.IsNewRow) return;
            HapusBarisDipilih();
        }

        // =========================================================
        // 3. TOTAL, DISKON & KEMBALIAN
        // =========================================================
        private void HitungTotalBelanja()
        {
            decimal totalKotor = 0m;

            foreach (DataGridViewRow row in dgvKeranjang.Rows)
            {
                if (row.IsNewRow) continue;
                totalKotor += InputHelper.AmbilDecimal(row.Cells[5].Value);
            }

            _totalBelanjaKotor = totalKotor;

            // 1. Diskon Member
            if (_diskonMemberPersen > 0m && totalKotor > 0m)
            {
                _diskonMemberNominal = BulatkanSubtotal(totalKotor * (_diskonMemberPersen / 100m));
            }
            else
            {
                _diskonMemberNominal = 0m;
            }

            // 2. Diskon Tambahan Manual
            _diskonManualNominal = 0m;
            if (txtDiskonManual.Text.Trim().Length > 0 && InputHelper.TryParseNominal(txtDiskonManual.Text, out decimal disMan, out _))
            {
                if (disMan > 0m)
                {
                    _diskonManualNominal = Math.Min(disMan, totalKotor - _diskonMemberNominal);
                }
            }

            _diskonTotalNominal = _diskonMemberNominal + _diskonManualNominal;
            _totalBelanja = Math.Max(0m, BulatkanSubtotal(totalKotor - _diskonTotalNominal));

            string infoDiskon = "";
            if (_diskonMemberNominal > 0m) infoDiskon += $" | Diskon Member: Rp {InputHelper.FormatNominal(_diskonMemberNominal)}";
            if (_diskonManualNominal > 0m) infoDiskon += $" | Diskon Tambahan: Rp {InputHelper.FormatNominal(_diskonManualNominal)}";

            lblSubtotalInfo.Text = $"Subtotal: Rp {InputHelper.FormatNominal(totalKotor)}" + infoDiskon;
            lblTotal.Text = InputHelper.FormatNominal(_totalBelanja);

            if (cmbMetodeBayar.SelectedItem is ItemPilihan item && item.Jenis == "NON_TUNAI")
            {
                txtBayar.Text = InputHelper.FormatNominal(_totalBelanja);
            }

            PerbaruiKembalian();
        }

        private void txtBayar_TextChanged(object sender, EventArgs e)
        {
            PerbaruiKembalian();
        }

        private void PerbaruiKembalian()
        {
            if (cmbMetodeBayar.SelectedItem is ItemPilihan item && item.Jenis == "TEMPO")
            {
                lblKembalian.Text = "0 (TEMPO)";
                return;
            }

            if (!InputHelper.TryParseNominal(txtBayar.Text, out decimal uangBayar, out _)
                || _totalBelanja <= 0m)
            {
                lblKembalian.Text = "0";
                return;
            }

            decimal kembalian = uangBayar - _totalBelanja;

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

            ItemPilihan? metode = cmbMetodeBayar.SelectedItem as ItemPilihan;
            string jenisMetode = metode?.Jenis ?? "TUNAI";

            ItemPilihan? member = cmbMember.SelectedItem as ItemPilihan;
            ItemPilihan? sales = cmbSales.SelectedItem as ItemPilihan;

            if (jenisMetode != "TUNAI")
            {
                TampilkanPeringatan("Metode pembayaran selain Tunai saat ini masih dalam tahap pengembangan (Under Development).\nSilakan gunakan metode Tunai.");
                cmbMetodeBayar.SelectedIndex = 0;
                return;
            }

            decimal uangBayar = 0m;
            decimal kembalian = 0m;

            if (jenisMetode == "TEMPO")
            {
                uangBayar = 0m;
                kembalian = 0m;
            }
            else if (jenisMetode == "NON_TUNAI")
            {
                uangBayar = _totalBelanja;
                kembalian = 0m;
            }
            else
            {
                if (!InputHelper.TryParseNominal(txtBayar.Text, out uangBayar, out string pesanBayar)
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

                kembalian = uangBayar - _totalBelanja;
            }

            try
            {
                string noNota = SimpanTransaksi(
                    uangBayar, kembalian,
                    member?.Id, sales?.Id, sales?.Tag ?? 0m,
                    metode?.Id, jenisMetode);

                string infoSukses = $"Transaksi Berhasil Disimpan!\n\n"
                    + $"Nomor Nota : {noNota}\n"
                    + $"Total      : Rp {InputHelper.FormatNominal(_totalBelanja)}\n"
                    + $"Metode     : {metode?.Teks ?? "Tunai"}\n";

                if (jenisMetode == "TEMPO")
                {
                    infoSukses += $"Status     : PIUTANG / TEMPO (Member: {member?.Teks})\n";
                }
                else
                {
                    infoSukses += $"Dibayar    : Rp {InputHelper.FormatNominal(uangBayar)}\n"
                               + $"Kembalian  : Rp {InputHelper.FormatNominal(kembalian)}\n";
                }

                // 1. Siapkan data struk untuk Epson TM-U220D & Cash Drawer
                var struk = new StrukData
                {
                    NoNota = noNota,
                    Waktu = DateTime.Now,
                    NamaKasir = !string.IsNullOrWhiteSpace(Session.NamaLengkap) ? Session.NamaLengkap : "Kasir",
                    NamaMember = member?.Id.HasValue == true ? member.Teks : null,
                    NamaSales = sales?.Id.HasValue == true ? sales.Teks : null,
                    MetodeBayar = metode?.Teks ?? "Tunai",
                    SubtotalKotor = _totalBelanjaKotor,
                    DiskonMember = _diskonMemberNominal,
                    DiskonTambahan = _diskonManualNominal,
                    TotalBayar = _totalBelanja,
                    UangDiterima = uangBayar,
                    Kembalian = kembalian,
                    PoinDidapat = (member?.Id.HasValue == true) ? (int)(_totalBelanja / 10000m) : 0,
                    Items = new List<StrukItem>()
                };

                foreach (DataGridViewRow r in dgvKeranjang.Rows)
                {
                    if (r.IsNewRow) continue;
                    struk.Items.Add(new StrukItem
                    {
                        Kode = r.Cells[0].Value?.ToString() ?? "",
                        Nama = r.Cells[1].Value?.ToString() ?? "",
                        HargaSatuan = InputHelper.AmbilDecimal(r.Cells[2].Value),
                        DiskonPersen = InputHelper.AmbilDecimal(r.Cells[3].Value),
                        Qty = InputHelper.AmbilDecimal(r.Cells[4].Value),
                        Subtotal = InputHelper.AmbilDecimal(r.Cells[5].Value),
                    });
                }

                // 2. Kirim perintah cetak struk & buka Cash Drawer via Epson TM-U220D / POS Printer
                bool printBerhasil = PosPrinterService.CetakStrukDanBukaDrawer(struk, null, out string printerErr);
                if (printBerhasil)
                {
                    infoSukses += "\n[OK] Struk dicetak & Cash Drawer terbuka otomatis.";
                }
                else if (!string.IsNullOrWhiteSpace(printerErr))
                {
                    infoSukses += $"\nCatatan Printer: {printerErr}";
                }

                MessageBox.Show(infoSukses, "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        internal string SimpanTransaksi(
            decimal uangDiterima, decimal kembalian,
            int? idMember, int? idSales, decimal komisiPersen,
            int? idMetodeBayar, string jenisMetode)
        {
            using SqliteConnection conn = _koneksi.GetConn();
            using SqliteTransaction transaksi = conn.BeginTransaction(deferred: false);

            try
            {
                int idUser = Session.UserId;
                string noNota = BuatNomorNota();

                const string queryTransaksi =
                    "INSERT INTO tb_transaksi "
                    + "(no_nota, id_user, total_bayar, diskon_total, uang_diterima, kembalian, id_sales, id_member, id_metode) "
                    + "VALUES (@noNota, @idUser, @totalBayar, @diskonTotal, @uangDiterima, @kembalian, @idSales, @idMember, @idMetode)";

                long idTransaksi;
                using (SqliteCommand cmdTrans = new(queryTransaksi, conn, transaksi))
                {
                    cmdTrans.Parameters.AddWithValue("@noNota", noNota);
                    cmdTrans.Parameters.AddWithValue("@idUser", idUser);
                    cmdTrans.Parameters.AddWithValue("@totalBayar", _totalBelanja);
                    cmdTrans.Parameters.AddWithValue("@diskonTotal", _diskonTotalNominal);
                    cmdTrans.Parameters.AddWithValue("@uangDiterima", uangDiterima);
                    cmdTrans.Parameters.AddWithValue("@kembalian", kembalian);
                    cmdTrans.Parameters.AddWithValue("@idSales", idSales.HasValue ? idSales.Value : DBNull.Value);
                    cmdTrans.Parameters.AddWithValue("@idMember", idMember.HasValue ? idMember.Value : DBNull.Value);
                    cmdTrans.Parameters.AddWithValue("@idMetode", idMetodeBayar.HasValue ? idMetodeBayar.Value : DBNull.Value);
                    cmdTrans.ExecuteNonQuery();

                    idTransaksi = AmbilIdBaruTerakhir(conn, transaksi);
                }

                const string queryDetail =
                    "INSERT INTO tb_detail_transaksi "
                    + "(id_transaksi, no_nota, kode_barcode, nama_barang, qty, "
                    + " harga_satuan, subtotal, harga_beli_satuan, tipe_harga, diskon_item) "
                    + "VALUES (@idTransaksi, @noNota, @kode, @nama, @qty, @harga, @subtotal, @hargaBeli, @tipeHarga, @diskonItem)";

                const string queryStok =
                    "UPDATE tb_barang SET stok = stok - @qty WHERE kode_barcode = @kode";

                const string queryCekStok =
                    "SELECT nama_barang, stok, harga_beli, harga_jual, "
                    + "COALESCE(minimal_grosir, 0) AS minimal_grosir, "
                    + "COALESCE(harga_grosir, 0) AS harga_grosir "
                    + "FROM tb_barang WHERE kode_barcode = @kode";

                const string queryMutasi =
                    "INSERT INTO tb_mutasi_stok "
                    + "(kode_barcode, tipe, qty, stok_akhir, keterangan, id_user, ref_no_nota) "
                    + "VALUES (@kode, 'KELUAR', @qty, @stokAkhir, @keterangan, @idUser, @noNota)";

                foreach (DataGridViewRow row in dgvKeranjang.Rows)
                {
                    if (row.IsNewRow) continue;

                    string kodeBarang = row.Cells[0].Value?.ToString() ?? string.Empty;
                    if (kodeBarang.Length == 0) continue;

                    decimal qty = InputHelper.AmbilDecimal(row.Cells[4].Value);
                    if (qty <= 0m) continue;

                    decimal hargaSatuan = InputHelper.AmbilDecimal(row.Cells[2].Value);
                    decimal diskonItem = InputHelper.AmbilDecimal(row.Cells[3].Value);
                    decimal subtotal = BulatkanSubtotal(InputHelper.AmbilDecimal(row.Cells[5].Value));

                    string namaBarang;
                    decimal stokTerbaru;
                    decimal hargaBeli;
                    decimal minGrosir;
                    decimal hargaGrosir;

                    using (SqliteCommand cmdCek = new(queryCekStok, conn, transaksi))
                    {
                        cmdCek.Parameters.AddWithValue("@kode", kodeBarang);
                        using SqliteDataReader reader = cmdCek.ExecuteReader();
                        if (!reader.Read())
                        {
                            throw new StokTidakCukupException(
                                "Barang \"" + kodeBarang + "\" tidak lagi ada di database.");
                        }

                        namaBarang = reader["nama_barang"]?.ToString() ?? kodeBarang;
                        stokTerbaru = InputHelper.AmbilDecimal(reader["stok"]);
                        hargaBeli = InputHelper.AmbilDecimal(reader["harga_beli"]);
                        minGrosir = InputHelper.AmbilDecimal(reader["minimal_grosir"]);
                        hargaGrosir = InputHelper.AmbilDecimal(reader["harga_grosir"]);
                    }

                    if (stokTerbaru < qty)
                    {
                        throw new StokTidakCukupException(
                            "Stok \"" + namaBarang + "\" tidak cukup!\n"
                            + "Diminta: " + InputHelper.FormatJumlah(qty)
                            + ", tersedia: " + InputHelper.FormatJumlah(stokTerbaru) + ".");
                    }

                    string tipeHarga = (minGrosir > 0 && hargaGrosir > 0 && qty >= minGrosir) ? "GROSIR" : "ECER";

                    using (SqliteCommand cmdDetail = new(queryDetail, conn, transaksi))
                    {
                        cmdDetail.Parameters.AddWithValue("@idTransaksi", idTransaksi);
                        cmdDetail.Parameters.AddWithValue("@noNota", noNota);
                        cmdDetail.Parameters.AddWithValue("@kode", kodeBarang);
                        cmdDetail.Parameters.AddWithValue("@nama", namaBarang);
                        cmdDetail.Parameters.AddWithValue("@qty", qty);
                        cmdDetail.Parameters.AddWithValue("@harga", hargaSatuan);
                        cmdDetail.Parameters.AddWithValue("@subtotal", subtotal);
                        cmdDetail.Parameters.AddWithValue("@hargaBeli", hargaBeli);
                        cmdDetail.Parameters.AddWithValue("@tipeHarga", tipeHarga);
                        cmdDetail.Parameters.AddWithValue("@diskonItem", diskonItem);
                        cmdDetail.ExecuteNonQuery();
                    }

                    using (SqliteCommand cmdStok = new(queryStok, conn, transaksi))
                    {
                        cmdStok.Parameters.AddWithValue("@qty", qty);
                        cmdStok.Parameters.AddWithValue("@kode", kodeBarang);
                        cmdStok.ExecuteNonQuery();
                    }

                    using (SqliteCommand cmdMutasi = new(queryMutasi, conn, transaksi))
                    {
                        cmdMutasi.Parameters.AddWithValue("@kode", kodeBarang);
                        cmdMutasi.Parameters.AddWithValue("@qty", -qty);
                        cmdMutasi.Parameters.AddWithValue("@stokAkhir", stokTerbaru - qty);
                        cmdMutasi.Parameters.AddWithValue("@keterangan", "Penjualan " + noNota);
                        cmdMutasi.Parameters.AddWithValue("@idUser", idUser);
                        cmdMutasi.Parameters.AddWithValue("@noNota", noNota);
                        cmdMutasi.ExecuteNonQuery();
                    }
                }

                // 1. Catat Komisi Sales jika sales dipilih dan ada komisi
                if (idSales.HasValue && komisiPersen > 0m && _totalBelanja > 0m)
                {
                    decimal jumlahKomisi = BulatkanSubtotal(_totalBelanja * (komisiPersen / 100m));
                    using SqliteCommand cmdKomisi = new(
                        "INSERT INTO tb_komisi (id_transaksi, no_nota, id_sales, nilai_nota, persen, jumlah_komisi, sudah_dibayar, id_user) "
                        + "VALUES (@idTrans, @nota, @sales, @total, @persen, @komisi, 0, @user)", conn, transaksi);
                    cmdKomisi.Parameters.AddWithValue("@idTrans", idTransaksi);
                    cmdKomisi.Parameters.AddWithValue("@nota",    noNota);
                    cmdKomisi.Parameters.AddWithValue("@sales",   idSales.Value);
                    cmdKomisi.Parameters.AddWithValue("@total",   _totalBelanja);
                    cmdKomisi.Parameters.AddWithValue("@persen",  komisiPersen);
                    cmdKomisi.Parameters.AddWithValue("@komisi",  jumlahKomisi);
                    cmdKomisi.Parameters.AddWithValue("@user",    idUser);
                    cmdKomisi.ExecuteNonQuery();
                }

                // 2. Catat Poin Member jika member dipilih (1 poin per Rp 10.000)
                if (idMember.HasValue && _totalBelanja >= 10000m)
                {
                    int poinDidapat = (int)(_totalBelanja / 10000m);
                    if (poinDidapat > 0)
                    {
                        using SqliteCommand cmdPoin = new(
                            "INSERT INTO tb_poin_log (id_member, tipe, jumlah_poin, id_transaksi, keterangan, id_user) "
                            + "VALUES (@m, 'TAMBAH', @poin, @idTrans, @ket, @u)", conn, transaksi);
                        cmdPoin.Parameters.AddWithValue("@m",       idMember.Value);
                        cmdPoin.Parameters.AddWithValue("@poin",    poinDidapat);
                        cmdPoin.Parameters.AddWithValue("@idTrans", idTransaksi);
                        cmdPoin.Parameters.AddWithValue("@ket",     "Poin transaksi " + noNota);
                        cmdPoin.Parameters.AddWithValue("@u",       idUser);
                        cmdPoin.ExecuteNonQuery();
                    }
                }

                // 3. Catat Piutang jika metode bayar TEMPO
                if (jenisMetode == "TEMPO" && idMember.HasValue)
                {
                    string jatuhTempo = DateTime.Today.AddDays(30).ToString("yyyy-MM-dd");
                    using SqliteCommand cmdPiutang = new(
                        "INSERT INTO tb_piutang (no_nota, id_transaksi, id_member, jatuh_tempo, jumlah_piutang, sudah_bayar, keterangan) "
                        + "VALUES (@nota, @idTrans, @member, @tempo, @jml, 0, 'Piutang penjualan kasir')", conn, transaksi);
                    cmdPiutang.Parameters.AddWithValue("@nota",    noNota);
                    cmdPiutang.Parameters.AddWithValue("@idTrans", idTransaksi);
                    cmdPiutang.Parameters.AddWithValue("@member",  idMember.Value);
                    cmdPiutang.Parameters.AddWithValue("@tempo",   jatuhTempo);
                    cmdPiutang.Parameters.AddWithValue("@jml",     _totalBelanja);
                    cmdPiutang.ExecuteNonQuery();
                }

                transaksi.Commit();
                return noNota;
            }
            catch
            {
                try { transaksi.Rollback(); } catch { }
                throw;
            }
        }

        private static long AmbilIdBaruTerakhir(SqliteConnection conn, SqliteTransaction? transaksi)
        {
            using SqliteCommand cmd = conn.CreateCommand();
            cmd.Transaction = transaksi;
            cmd.CommandText = "SELECT last_insert_rowid()";
            return Convert.ToInt64(cmd.ExecuteScalar());
        }

        private static string BuatNomorNota()
        {
            return "TRX-" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
        }

        private void ResetTransaksi()
        {
            dgvKeranjang.Rows.Clear();
            _totalBelanjaKotor = 0m;
            _totalBelanja = 0m;
            _diskonMemberNominal = 0m;
            _diskonManualNominal = 0m;
            _diskonTotalNominal = 0m;
            lblTotal.Text = "0";
            lblSubtotalInfo.Text = "Subtotal: Rp 0";
            lblKembalian.Text = "0";
            txtDiskonManual.Clear();
            txtBayar.Clear();
            txtBarcode.Clear();
            if (cmbMember.Items.Count > 0) cmbMember.SelectedIndex = 0;
            if (cmbSales.Items.Count > 0) cmbSales.SelectedIndex = 0;
            if (cmbMetodeBayar.Items.Count > 0) cmbMetodeBayar.SelectedIndex = 0;
            txtBarcode.Focus();
        }

        // =========================================================
        // 5. TAMPILAN
        // =========================================================
        private void dgvKeranjang_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex < 0 || e.RowIndex < 0) return;

            object? nilai = dgvKeranjang.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
            if (nilai is null) return;

            decimal angka = InputHelper.AmbilDecimal(nilai);

            e.Value = e.ColumnIndex switch
            {
                2 or 5 => InputHelper.FormatNominal(angka),
                3 => angka > 0m ? $"{angka:0.##}%" : "-",
                4 => InputHelper.FormatJumlah(angka),
                _ => e.Value,
            };

            e.FormattingApplied = e.ColumnIndex is 2 or 3 or 4 or 5;
        }

        private void FormKasir_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (!_sedangKeluar && IsRootForm)
            {
                _sedangKeluar = true;
                Session.Clear();
                Application.Exit();
            }
        }
    }

    internal sealed class StokTidakCukupException : Exception
    {
        public StokTidakCukupException(string message) : base(message)
        {
        }
    }
}

