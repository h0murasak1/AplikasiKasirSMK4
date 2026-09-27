namespace AplikasiKasirSMK4
{
    /// <summary>
    /// Dialog sederhana untuk meminta satu nilai teks dari pengguna.
    /// Dipakai untuk mengoreksi jumlah (qty) barang di keranjang, sehingga
    /// kasir tidak perlu memindai barcode berulang kali untuk menjual
    ///misalnya 3 pcs dalam satu kali.
    /// Dibuat lewat kode, bukan designer, agar tidak menambah berkas .resx.
    /// </summary>
    internal sealed class InputDialog : Form
    {
        private readonly TextBox _txtNilai;

        private InputDialog(string judul, string label, string nilaiAwal, string perintah)
        {
            Text = judul;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(360, 165);
            Font = new Font("Segoe UI", 10F);
            BackColor = Color.White;

            Label lbl = new()
            {
                Text = label,
                Location = new Point(20, 18),
                AutoSize = true,
                ForeColor = Color.FromArgb(48, 49, 51),
            };

            _txtNilai = new TextBox
            {
                Location = new Point(20, 44),
                Width = 320,
                Font = new Font("Segoe UI", 14F),
                Text = nilaiAwal,
                TextAlign = HorizontalAlignment.Right,
            };
            // Dialog ini khusus untuk mengoreksi qty, jadi isinya dikunci ke
            // angka bulat sejak ketikan pertama. Format ribuan otomatis
            // sengaja tidak dipakai: dulu "1,5" dibuang komainya menjadi
            // "15" tanpa pesan, sehingga qty jadi sepuluh kali lipat.
            _txtNilai.KeyPress += (_, e) => e.Handled = !InputHelper.BolehMasukAngka(e.KeyChar);
            _txtNilai.SelectAll();

            Label lblBantuan = new()
            {
                Text = perintah,
                Location = new Point(20, 80),
                Size = new Size(320, 40),
                ForeColor = Color.FromArgb(144, 147, 153),
                Font = new Font("Segoe UI", 8F),
            };

            Button btnOke = new()
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Location = new Point(176, 126),
                Width = 80,
                Height = 30,
            };
            btnOke.Click += (_, _) => OK_Click();

            Button btnBatal = new()
            {
                Text = "Batal",
                DialogResult = DialogResult.Cancel,
                Location = new Point(260, 126),
                Width = 80,
                Height = 30,
            };

            Controls.AddRange(new Control[] { lbl, _txtNilai, lblBantuan, btnOke, btnBatal });
            AcceptButton = btnOke;
            CancelButton = btnBatal;
        }

        /// <summary>Isi yang diketik pengguna, tanpa spasi di tepi.</summary>
        public string Nilai => _txtNilai.Text.Trim();

        /// <summary>
        /// Menampilkan dialog lalu mengembalikan true bila pengguna menekan OK.
        /// </summary>
        public static bool Tanyakan(
            string judul, string label, string nilaiAwal, string perintah, out string hasil)
        {
            using InputDialog dialog = new(judul, label, nilaiAwal, perintah);
            hasil = dialog.Nilai;

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                hasil = string.Empty;
                return false;
            }

            hasil = dialog.Nilai;
            return true;
        }

        private void OK_Click()
        {
            if (Nilai.Length == 0)
            {
                MessageBox.Show(this, "Isi terlebih dahulu.", "Peringatan",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
