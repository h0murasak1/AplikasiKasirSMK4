namespace AplikasiKasirSMK4
{
    /// <summary>
    /// Pemilih master data yang akan dikelola.
    /// </summary>
    public class FormMasterMenu : Form
    {
        public FormMasterMenu()
        {
            Text = "Master Data & Pengaturan Toko";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.None;
            Dock = DockStyle.Fill;
            BackColor = Color.FromArgb(248, 250, 252);
            Font = new Font("Segoe UI", 9.5F);

            BangunTampilan();
        }

        private void BangunTampilan()
        {
            var pnlPusat = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(24),
                BackColor = Color.Transparent,
                AutoScroll = true
            };

            var card = new Panel
            {
                Width = 840,
                Height = 440,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(24)
            };

            var lblHeader = new Label
            {
                Text = "⚙️ PENGELOLAAN MASTER DATA & SISTEM",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42),
                Location = new Point(24, 20),
                AutoSize = true
            };

            var lblSub = new Label
            {
                Text = "Pilih modul data atau konfigurasi perangkat yang ingin dikelola:",
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(100, 116, 139),
                Location = new Point(26, 50),
                AutoSize = true
            };

            var tabel = new TableLayoutPanel
            {
                Location = new Point(24, 85),
                Width = 790,
                Height = 320,
                ColumnCount = 2,
                RowCount = 4,
                BackColor = Color.Transparent
            };
            tabel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tabel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            for (int i = 0; i < 4; i++)
            {
                tabel.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            }

            int pos = 0;

            // 1. Profil Toko & PPN
            TambahTile(tabel, pos++, "🏬 Profil Toko & PPN", "Identitas toko, nomor kontak, NPWP, struk & PPN",
                Color.FromArgb(37, 99, 235), (_, _) =>
                {
                    using FormProfilToko f = new();
                    f.ShowDialog(this);
                });

            // 2. Manajemen User
            TambahTile(tabel, pos++, "👤 Manajemen User", "Kelola akun kasir, admin, reset sandi & role",
                Color.FromArgb(124, 58, 237), (_, _) =>
                {
                    using FormUser f = new();
                    f.ShowDialog(this);
                });

            // 3. Data Member & Poin
            TambahTile(tabel, pos++, "💳 Data Member & Poin", "Pelanggan loyal, diskon member, & riwayat poin",
                Color.FromArgb(217, 119, 6), (_, _) =>
                {
                    using FormMember f = new();
                    f.ShowDialog(this);
                });

            // 4. Data Sales
            TambahTile(tabel, pos++, "👔 Data Sales & Komisi", "Daftar sales, nomor telepon, & persentase komisi",
                Color.FromArgb(13, 148, 136), (_, _) =>
                {
                    using FormMaster f = FormMaster.Untuk(3);
                    f.ShowDialog(this);
                });

            // 5. Jenis Barang
            TambahTile(tabel, pos++, "🏷️ Jenis Barang (Kategori)", "Pengelompokan jenis dan kategori produk",
                Color.FromArgb(5, 150, 105), (_, _) =>
                {
                    using FormMaster f = FormMaster.Untuk(0);
                    f.ShowDialog(this);
                });

            // 6. Merek Barang
            TambahTile(tabel, pos++, "✨ Merek Barang", "Daftar brand dan merek dagang produk",
                Color.FromArgb(2, 132, 199), (_, _) =>
                {
                    using FormMaster f = FormMaster.Untuk(1);
                    f.ShowDialog(this);
                });

            // 7. Data Supplier
            TambahTile(tabel, pos++, "🚚 Data Supplier", "Daftar pemasok, kontak, dan alamat supplier",
                Color.FromArgb(71, 85, 105), (_, _) =>
                {
                    using FormMaster f = FormMaster.Untuk(2);
                    f.ShowDialog(this);
                });

            // 8. Pengaturan Printer & Drawer
            TambahTile(tabel, pos++, "🖨️ Printer & Cash Drawer", "Konfigurasi Epson TM-U220D & tes laci kasir",
                Color.FromArgb(15, 23, 42), (_, _) =>
                {
                    using FormPengaturanPrinter f = new();
                    f.ShowDialog(this);
                });

            card.Controls.AddRange(new Control[] { lblHeader, lblSub, tabel });
            pnlPusat.Controls.Add(card);

            // Auto center card on resize
            pnlPusat.Resize += (_, _) =>
            {
                card.Location = new Point(
                    Math.Max(16, (pnlPusat.ClientSize.Width - card.Width) / 2),
                    Math.Max(16, (pnlPusat.ClientSize.Height - card.Height) / 2)
                );
            };

            Controls.Add(pnlPusat);
        }

        private static void TambahTile(TableLayoutPanel tabel, int pos, string judul, string sub, Color warna, EventHandler aksi)
        {
            var btn = new Button
            {
                Text = judul + "\n" + sub,
                Dock = DockStyle.Fill,
                Margin = new Padding(6),
                FlatStyle = FlatStyle.Flat,
                BackColor = warna,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 4, 12, 4),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = ControlPaint.Light(warna, 0.12f);
            btn.Click += aksi;

            tabel.Controls.Add(btn, pos % 2, pos / 2);
        }
    }
}