namespace AplikasiKasirSMK4
{
    /// <summary>
    /// Form dialog untuk memilih printer POS (misal EPSON TM-U220D) dan menguji Cash Drawer.
    /// </summary>
    public class FormPengaturanPrinter : Form
    {
        private readonly ComboBox cmbPrinter = new();
        private readonly Button btnSimpan = new();
        private readonly Button btnBukaDrawer = new();
        private readonly Button btnTestPrint = new();
        private readonly Button btnBatal = new();
        private readonly Label lblStatus = new();

        public FormPengaturanPrinter()
        {
            Text = "Pengaturan Printer & Cash Drawer";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(520, 290);
            Font = new Font("Segoe UI", 9.5F);

            BangunTampilan();
            MuatDaftarPrinter();
        }

        private void BangunTampilan()
        {
            var pnl = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16) };

            var lblJudul = new Label
            {
                Text = "Konfigurasi Printer Kasir & Cash Drawer",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Location = new Point(16, 14),
                AutoSize = true
            };

            var lblPilih = new Label
            {
                Text = "Pilih Printer (misal: EPSON TM-U220 / TM-U220D / POS):",
                Location = new Point(16, 54),
                AutoSize = true
            };

            cmbPrinter.Location = new Point(16, 80);
            cmbPrinter.Width = 470;
            cmbPrinter.DropDownStyle = ComboBoxStyle.DropDownList;

            lblStatus.Location = new Point(16, 120);
            lblStatus.Size = new Size(470, 40);
            lblStatus.ForeColor = Color.FromArgb(41, 128, 185);

            // Tombol Test Drawer
            btnBukaDrawer.Text = "⚡ Tes Buka Drawer";
            btnBukaDrawer.Location = new Point(16, 170);
            btnBukaDrawer.Size = new Size(160, 36);
            btnBukaDrawer.FlatStyle = FlatStyle.Flat;
            btnBukaDrawer.BackColor = Color.FromArgb(230, 126, 34);
            btnBukaDrawer.ForeColor = Color.White;
            btnBukaDrawer.Cursor = Cursors.Hand;
            btnBukaDrawer.FlatAppearance.BorderSize = 0;
            btnBukaDrawer.Click += BtnBukaDrawer_Click;

            // Tombol Test Print
            btnTestPrint.Text = "🖨️ Tes Cetak Struk";
            btnTestPrint.Location = new Point(186, 170);
            btnTestPrint.Size = new Size(160, 36);
            btnTestPrint.FlatStyle = FlatStyle.Flat;
            btnTestPrint.BackColor = Color.FromArgb(52, 152, 219);
            btnTestPrint.ForeColor = Color.White;
            btnTestPrint.Cursor = Cursors.Hand;
            btnTestPrint.FlatAppearance.BorderSize = 0;
            btnTestPrint.Click += BtnTestPrint_Click;

            // Tombol Simpan
            btnSimpan.Text = "✓ Simpan Pilihan";
            btnSimpan.Location = new Point(236, 230);
            btnSimpan.Size = new Size(140, 36);
            btnSimpan.FlatStyle = FlatStyle.Flat;
            btnSimpan.BackColor = Color.FromArgb(39, 174, 96);
            btnSimpan.ForeColor = Color.White;
            btnSimpan.Cursor = Cursors.Hand;
            btnSimpan.FlatAppearance.BorderSize = 0;
            btnSimpan.Click += BtnSimpan_Click;

            // Tombol Batal
            btnBatal.Text = "Batal";
            btnBatal.Location = new Point(386, 230);
            btnBatal.Size = new Size(100, 36);
            btnBatal.FlatStyle = FlatStyle.Flat;
            btnBatal.BackColor = Color.FromArgb(120, 120, 120);
            btnBatal.ForeColor = Color.White;
            btnBatal.Cursor = Cursors.Hand;
            btnBatal.FlatAppearance.BorderSize = 0;
            btnBatal.Click += (_, _) => Close();

            pnl.Controls.AddRange(new Control[]
            {
                lblJudul, lblPilih, cmbPrinter, lblStatus,
                btnBukaDrawer, btnTestPrint, btnSimpan, btnBatal
            });

            Controls.Add(pnl);
        }

        private void MuatDaftarPrinter()
        {
            cmbPrinter.Items.Clear();
            var list = PosPrinterService.AmbilDaftarPrinterTerpasang();

            if (list.Count == 0)
            {
                lblStatus.Text = "Tidak ada printer yang terpasang di Windows. Pastikan kabel printer USB/Serial tersambung dan driver terinstal.";
                lblStatus.ForeColor = Color.Red;
                btnSimpan.Enabled = false;
                btnBukaDrawer.Enabled = false;
                btnTestPrint.Enabled = false;
                return;
            }

            string aktif = PosPrinterService.AmbilNamaPrinter();
            int selectedIndex = 0;

            for (int i = 0; i < list.Count; i++)
            {
                cmbPrinter.Items.Add(list[i]);
                if (string.Equals(list[i], aktif, StringComparison.OrdinalIgnoreCase))
                {
                    selectedIndex = i;
                }
            }

            cmbPrinter.SelectedIndex = selectedIndex;
            lblStatus.Text = $"Printer aktif saat ini: \"{cmbPrinter.SelectedItem}\"";
        }

        private void BtnBukaDrawer_Click(object? sender, EventArgs e)
        {
            string? printer = cmbPrinter.SelectedItem?.ToString();
            if (string.IsNullOrWhiteSpace(printer)) return;

            lblStatus.Text = "Mengirim sinyal buka cash drawer ke " + printer + "...";
            lblStatus.ForeColor = Color.FromArgb(41, 128, 185);

            if (PosPrinterService.BukaCashDrawer(printer, out string err))
            {
                lblStatus.Text = "✓ Perintah buka cash drawer berhasil dikirim!";
                lblStatus.ForeColor = Color.FromArgb(39, 174, 96);
            }
            else
            {
                lblStatus.Text = "✕ Gagal membuka cash drawer: " + err;
                lblStatus.ForeColor = Color.Red;
            }
        }

        private void BtnTestPrint_Click(object? sender, EventArgs e)
        {
            string? printer = cmbPrinter.SelectedItem?.ToString();
            if (string.IsNullOrWhiteSpace(printer)) return;

            lblStatus.Text = "Mencetak struk contoh ke " + printer + "...";
            lblStatus.ForeColor = Color.FromArgb(41, 128, 185);

            var sample = new StrukData
            {
                NoNota = "TEST-" + DateTime.Now.ToString("HHmmss"),
                Waktu = DateTime.Now,
                NamaKasir = Session.NamaLengkap.Length > 0 ? Session.NamaLengkap : "Administrator",
                MetodeBayar = "TUNAI",
                SubtotalKotor = 25000m,
                TotalBayar = 25000m,
                UangDiterima = 50000m,
                Kembalian = 25000m,
                Items = new List<StrukItem>
                {
                    new StrukItem
                    {
                        Kode = "BRG001",
                        Nama = "Buku Tulis Sinar Dunia 38lbr",
                        Qty = 2,
                        HargaSatuan = 5000m,
                        Subtotal = 10000m
                    },
                    new StrukItem
                    {
                        Kode = "BRG002",
                        Nama = "Pulpen Gel Pilot G2 0.5 Black",
                        Qty = 1,
                        HargaSatuan = 15000m,
                        Subtotal = 15000m
                    }
                }
            };

            if (PosPrinterService.CetakStrukDanBukaDrawer(sample, printer, out string err))
            {
                lblStatus.Text = "✓ Cetak struk contoh & sinyal drawer berhasil dikirim!";
                lblStatus.ForeColor = Color.FromArgb(39, 174, 96);
            }
            else
            {
                lblStatus.Text = "✕ Gagal mencetak: " + err;
                lblStatus.ForeColor = Color.Red;
            }
        }

        private void BtnSimpan_Click(object? sender, EventArgs e)
        {
            string? printer = cmbPrinter.SelectedItem?.ToString();
            if (!string.IsNullOrWhiteSpace(printer))
            {
                PosPrinterService.SimpanNamaPrinter(printer);
                MessageBox.Show($"Printer \"{printer}\" berhasil disimpan sebagai printer kasir utama.",
                    "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
            }
        }
    }
}
