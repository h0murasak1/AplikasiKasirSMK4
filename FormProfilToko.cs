using Microsoft.Data.Sqlite;

namespace AplikasiKasirSMK4
{
    /// <summary>
    /// Form untuk mengisi dan mengubah profil toko.
    /// </summary>
    /// <remarks>
    /// Bedanya dengan master lain: tabelnya hanya boleh berisi satu
    /// baris. Karena itu form ini tidak punya daftar dan tidak punya
    /// tombol tambah. Yang ada hanya isi, simpan, dan kembalikan.
    /// <para>
    /// Kolom PPN ikut di form yang sama, bukan form terpisah, karena
    /// persen PPN adalah keputusan toko. Menaruhnya di form lain
    /// membuat orang mengira PPN itu pengaturan aplikasi, bukan
    /// pengaturan toko.
    /// </para>
    /// <para>
    /// Bentukannya dibuat di kode seperti FormMaster, karena jumlah
    /// kolomnya diketahui dan tidak berubah. Tidak perlu desainer.
    /// </para>
    /// </remarks>
    public class FormProfilToko : Form
    {
        private readonly Koneksi _koneksi = new();

        private readonly TextBox txtNamaToko = new();
        private readonly TextBox txtNamaPemilik = new();
        private readonly TextBox txtTelepon = new();
        private readonly TextBox txtEmail = new();
        private readonly TextBox txtNpwp = new();
        private readonly TextBox txtAlamat = new();
        private readonly TextBox txtCatatanStruk = new();
        private readonly CheckBox chkPpnAktif = new();
        private readonly NumericUpDown nudPpnPersen = new();
        private readonly Label lblPpn = new();

        public FormProfilToko()
        {
            Text = "Profil Toko";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(640, 540);
            Font = new Font("Segoe UI", 9F);

            BangunTampilan();
        }

        private void BangunTampilan()
        {
            var utama = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(14)
            };
            utama.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            utama.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            utama.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            utama.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            Controls.Add(utama);
            utama.Controls.Add(BangunJudul(), 0, 0);
            utama.Controls.Add(BangunIsian(), 0, 1);
            utama.Controls.Add(BangunTombol(), 0, 2);
        }

        private Control BangunJudul()
        {
            return new Label
            {
                Text = "Profil Toko",
                Dock = DockStyle.Top,
                AutoSize = false,
                Height = 36,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                ForeColor = Color.FromArgb(41, 128, 185),
                Margin = new Padding(0, 0, 0, 8)
            };
        }

        /// <summary>
        /// Membangun daftar isian profil toko.
        /// </summary>
        /// <remarks>
        /// Tiga kolom: label di kiri, isian di tengah, dan contoh atau
        /// catatan singkat di kanan. Kolom ketiga ini ada supaya kasir
        /// tidak perlu membuka dokumentasi untuk tahu apa arti satu
        /// kolom, misalnya kapan PPN harus diisi.
        /// </remarks>
        private Control BangunIsian()
        {
            var tabel = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 3
            };
            tabel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tabel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tabel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190F));

            int baris = 0;

            baris = TambahIsian(tabel, baris, "Nama Toko *", txtNamaToko,
                "Wajib. Ditampilkan di struk.");

            baris = TambahIsian(tabel, baris, "Nama Pemilik", txtNamaPemilik,
                "Boleh dikosongkan.");

            baris = TambahIsian(tabel, baris, "Telepon", txtTelepon,
                "Boleh dikosongkan.");

            baris = TambahIsian(tabel, baris, "Email", txtEmail,
                "Boleh dikosongkan.");

            baris = TambahIsian(tabel, baris, "NPWP", txtNpwp,
                "Untuk laporan pajak.");

            baris = TambahIsianTinggi(tabel, baris, "Alamat", txtAlamat, 70,
                "Boleh dikosongkan.");

            baris = TambahIsianTinggi(tabel, baris, "Catatan Struk", txtCatatanStruk, 60,
                "Dicetak di bagian bawah struk.");

            baris = TambahIsian(tabel, baris, "Pakai PPN", chkPpnAktif,
                "Centang kalau toko memang menarik PPN.");

            baris = TambahIsian(tabel, baris, "Persen PPN", nudPpnPersen,
                "Contoh: 11 berarti sebelas persen.");

            return tabel;
        }

        /// <summary>
        /// Menambah satu baris isian sebaris, lalu mengembalikan nomor
        /// baris berikutnya.
        /// </summary>
        /// <remarks>
        /// Panjang maksimum teks dipasang di sini, bukan dibiarkan
        /// tak terbatas. Kolom di database tidak punya batas panjang,
        /// jadi tanpa batas di aplikasi pun, satu isian bisa memuat
        /// seluruh paragraf dan membuat tampilan form berantakan.
        /// </remarks>
        private static int TambahIsian(
            TableLayoutPanel tabel, int baris, string label, Control isian,
            string catatan)
        {
            if (isian is TextBox kotak)
            {
                kotak.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            }
            else if (isian is CheckBox centang)
            {
                centang.AutoSize = true;
                centang.Anchor = AnchorStyles.Left;
            }
            else if (isian is NumericUpDown angka)
            {
                angka.Anchor = AnchorStyles.Left;

                // Persen PPN dibatasi 0 sampai 100 dengan dua desimal,
                // sama persis dengan CHECK di database. Batas yang sama
                // di dua tempat supaya tidak ada angka yang bisa
                // diketik tapi tidak bisa disimpan.
                angka.Minimum = 0m;
                angka.Maximum = 100m;
                angka.DecimalPlaces = 2;
                angka.Increment = 1m;
                angka.Width = 90;
            }

            tabel.Controls.Add(BuatLabel(label), 0, baris);
            tabel.Controls.Add(isian, 1, baris);
            tabel.Controls.Add(BuatCatatan(catatan), 2, baris);
            return baris + 1;
        }

        /// <summary>
        /// Menambah satu baris isian yang kotaknya beberapa baris tinggi.
        /// </summary>
        private static int TambahIsianTinggi(
            TableLayoutPanel tabel, int baris, string label, TextBox isian,
            int tinggi, string catatan)
        {
            isian.Multiline = true;
            isian.ScrollBars = ScrollBars.Vertical;
            isian.Height = tinggi;

            return TambahIsian(tabel, baris, label, isian, catatan);
        }

        private static Label BuatLabel(string teks)
        {
            return new Label
            {
                Text = teks,
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                ForeColor = Color.FromArgb(60, 60, 60),
                Margin = new Padding(0, 7, 10, 0)
            };
        }

        private static Label BuatCatatan(string teks)
        {
            return new Label
            {
                Text = teks,
                AutoSize = true,
                MaximumSize = new Size(190, 0),
                ForeColor = Color.Gray,
                Margin = new Padding(6, 7, 0, 0)
            };
        }

        /// <summary>
        /// Mengatur MaxLength untuk seluruh kotak teks di form ini.
        /// </summary>
        /// <remarks>
        /// Nilainya diambil dari definisi kolom TEXT di skema, bukan
        /// ditebak. Kolom yang panjangnya tidak dibatasi database,
        /// misalnya nama toko, dibatasi juga di sini supaya tidak
        /// mungkin berisi isian yang sangat besar.
        /// </remarks>
        private void AturBatasPanjang()
        {
            txtNamaToko.MaxLength = 100;
            txtNamaPemilik.MaxLength = 100;
            txtTelepon.MaxLength = 30;
            txtEmail.MaxLength = 120;
            txtNpwp.MaxLength = 30;
            txtAlamat.MaxLength = 300;
            txtCatatanStruk.MaxLength = 300;
        }

        private Control BangunTombol()
        {
            var panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0, 10, 0, 0)
            };

            var tutup = new Button
            {
                Text = "Tutup",
                AutoSize = true,
                Height = 34,
                Padding = new Padding(16, 0, 16, 0),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(90, 90, 90),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            tutup.FlatAppearance.BorderSize = 0;
            tutup.Click += (_, _) => Close();

            var simpan = new Button
            {
                Text = "Simpan",
                AutoSize = true,
                Height = 34,
                Padding = new Padding(20, 0, 20, 0),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            simpan.FlatAppearance.BorderSize = 0;
            simpan.Click += btnSimpan_Click;

            // FlowDirection RightToLeft berarti tombol yang ditambahkan
            // lebih dulu muncul paling kanan. Yang disimpan sudah di
            // tangan kanan supaya tidak tertimpa saat jendela ditutup.
            panel.Controls.Add(simpan);
            panel.Controls.Add(tutup);
            return panel;
        }

        // ------------------------------------------------------------------
        // Membaca profil yang sudah tersimpan
        // ------------------------------------------------------------------

        /// <summary>
        /// Memuat isi profil toko dari database ke form.
        /// </summary>
        /// <remarks>
        /// Tabel ini dibatasi satu baris lewat CHECK (id_toko = 1),
        /// tapi barisnya belum tentu ada. Database yang baru dibuat
        /// sudah diberi satu baris dari migrasi, sedangkan database
        /// yang belum pernah dimigrasikan mungkin belum punya.
        /// Karena itu SELECT dibungkus penanganan "tidak ada baris",
        /// dan form tetap bisa dibuka: isian dikosongkan, dan menyimpan
        /// akan membuat barisnya.
        /// </remarks>
        private void MuatProfil()
        {
            try
            {
                using SqliteConnection conn = _koneksi.GetConn();

                using SqliteCommand cmd = new(SqlProfilToko.Baca(), conn);
                using SqliteDataReader r = cmd.ExecuteReader();

                if (!r.Read())
                {
                    return;
                }

                txtNamaToko.Text = AmbilTeks(r, "nama_toko");
                txtNamaPemilik.Text = AmbilTeks(r, "nama_pemilik");
                txtAlamat.Text = AmbilTeks(r, "alamat");
                txtTelepon.Text = AmbilTeks(r, "telepon");
                txtEmail.Text = AmbilTeks(r, "email");
                txtNpwp.Text = AmbilTeks(r, "npwp");
                txtCatatanStruk.Text = AmbilTeks(r, "catatan_struk");

                chkPpnAktif.Checked = InputHelper.AmbilInt(r["ppn_aktif"]) == 1;
                nudPpnPersen.Value = AmbilPersenPpn(r["ppn_persen"]);

                // Kotak persen mengikuti centang PPN. Persennya tidak
                // dihapus, hanya dikunci, supaya kalau centang dibuka
                // lagi angkanya masih ada di situ.
                PerbaruiStatusPpn();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Gagal memuat profil toko: " + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private static string AmbilTeks(SqliteDataReader r, string kolom)
        {
            object? nilai = r[kolom];
            return nilai is null || nilai is DBNull
                ? string.Empty
                : nilai.ToString() ?? string.Empty;
        }


        /// <summary>
        /// Menaruh nilai persen PPN ke dalam NumericUpDown dengan aman.
        /// </summary>
        /// <remarks>
        /// NumericUpDown punya batas 0 sampai 100. Nilai yang di luar
        /// batas itu akan melempar galat ketika dibaca dari kotak,
        /// sehingga form tertutup dengan pesan yang tidak menjelaskan
        /// apa yang sebenarnya salah. Karena itu nilainya diperiksa
        /// lebih dulu, dan penyimpangan dari batas dilaporkan ke kasir
        /// alih-alih dibuang diam-diam.
        /// </remarks>
        private decimal AmbilPersenPpn(object? nilai)
        {
            decimal persen = nilai is null || nilai is DBNull
                ? 0m
                : InputHelper.AmbilDecimal(nilai);

            if (persen is < 0m or > 100m)
            {
                MessageBox.Show(
                    "Persen PPN yang tersimpan adalah "
                    + InputHelper.FormatNominal(persen)
                    + ", di luar batas 0 sampai 100."
                    + Environment.NewLine
                    + "Nilai itu tidak dipakai. Isi ulang di sini, lalu simpan.",
                    "Data Tidak Valid",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return 0m;
            }

            return persen;
        }

        /// <summary>
        /// Mengaktifkan atau menonaktifkan kotak persen PPN.
        /// </summary>
        /// <remarks>
        /// Kotak persen tidak dihapus datanya saat PPN dimatikan,
        /// hanya dikunci. Kalau centang dibuka lagi, angka yang lalu
        /// masih ada, jadi orang tidak perlu mencari tahu lagi.
        /// </remarks>
        private void PerbaruiStatusPpn()
        {
            nudPpnPersen.Enabled = chkPpnAktif.Checked;
        }

        private void chkPpnAktif_CheckedChanged(object? sender, EventArgs e)
        {
            PerbaruiStatusPpn();

            // Sebelas persen diisi otomatis saat PPN dinyalakan
            // pertama kali. Alasannya, sebelas persen adalah nilai
            // yang paling sering dipakai di Indonesia, jadi kolom
            // akan menghasilkan PPN hidup dengan persen nol, dan
            //chk_profil_ppn di database akan menolak penyimpanan itu.
            // Pakai 11 lebih baik daripada diisi nol lalu ditolak.
            if (chkPpnAktif.Checked && nudPpnPersen.Value == 0m)
            {
                nudPpnPersen.Value = 11m;
            }
        }

        // ------------------------------------------------------------------
        // Menyimpan profil
        // ------------------------------------------------------------------

        private void btnSimpan_Click(object? sender, EventArgs e)
        {
            string namaToko = txtNamaToko.Text.Trim();
            if (namaToko.Length == 0)
            {
                MessageBox.Show(
                    "Nama toko tidak boleh kosong!",
                    "Peringatan",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                txtNamaToko.Focus();
                return;
            }

            bool ppnAktif = chkPpnAktif.Checked;
            decimal ppnPersen = ppnAktif ? nudPpnPersen.Value : 0m;

            if (ppnAktif && ppnPersen <= 0m)
            {
                // chk_profil_ppn di database sudah melarang ini juga, tapi
                // pesannya berbahasa SQLite. Dicegah di sini supaya
                // kasir langsung tahu harus mengisinya berapa.
                MessageBox.Show(
                    "PPN sudah dicentang, jadi persennya harus lebih dari nol.",
                    "Peringatan",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                nudPpnPersen.Focus();
                return;
            }

            try
            {
                using SqliteConnection conn = _koneksi.GetConn();

                SimpanProfil(conn, namaToko, ppnAktif, ppnPersen);

                MessageBox.Show(
                    "Profil toko berhasil disimpan.",
                    "Sukses",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                Close();
            }
            catch (SqliteException ex) when (SqliteError.AdalahCheck(ex))
            {
                MessageBox.Show(
                    "Nilai profil ditolak oleh aturan database."
                    + Environment.NewLine + ex.Message,
                    "Peringatan",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Gagal menyimpan profil toko: " + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Menyimpan profil toko ke satu baris tb_profil_toko.
        /// </summary>
        /// <remarks>
        /// Dipakai dua perintah, bukan satu. Alasannya, tabel ini
        /// dibatasi satu baris oleh CHECK (id_toko = 1), dan cara
        /// paling jujur untuk hal seperti itu adalah menyimpan saat
        /// barisnya sudah ada, lalu menyisipkan saat belum ada.
        /// </remarks>
        private void SimpanProfil(
            SqliteConnection conn, string namaToko, bool ppnAktif, decimal ppnPersen)
        {
            using (SqliteCommand cmd = new(SqlProfilToko.Perbarui(), conn))
            {
                PasangTeks(cmd);
                cmd.Parameters.AddWithValue("@nama", namaToko);
                cmd.Parameters.AddWithValue("@ppnAktif", ppnAktif ? 1 : 0);
                cmd.Parameters.AddWithValue("@ppnPersen", ppnPersen);

                if (cmd.ExecuteNonQuery() > 0)
                {
                    return;
                }
            }

            using SqliteCommand tambah = new(SqlProfilToko.Sisip(), conn);
            PasangTeks(tambah);
            tambah.Parameters.AddWithValue("@nama", namaToko);
            tambah.Parameters.AddWithValue("@ppnAktif", ppnAktif ? 1 : 0);
            tambah.Parameters.AddWithValue("@ppnPersen", ppnPersen);
            tambah.ExecuteNonQuery();
        }

        private void PasangTeks(SqliteCommand cmd)
        {
            SqlProfilToko.PasangTeks(cmd,
                txtNamaPemilik.Text, txtAlamat.Text, txtTelepon.Text,
                txtEmail.Text, txtNpwp.Text, txtCatatanStruk.Text);
        }

        // ------------------------------------------------------------------
        // Siklus hidup
        // ------------------------------------------------------------------

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Guard: profil toko menentukan nama, alamat, dan PPN yang
            // tercetak di struk, jadi hanya Admin yang boleh mengubahnya.
            // Kasir tetap boleh membaca hasilnya lewat struk dan laporan.
            if (!Session.IsAdmin)
            {
                MessageBox.Show(
                    "Hanya akun Admin yang dapat mengubah profil toko.",
                    "Akses Ditolak",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                Close();
                return;
            }

            AturBatasPanjang();
            chkPpnAktif.CheckedChanged += chkPpnAktif_CheckedChanged;
            MuatProfil();

            txtNamaToko.Focus();
        }
    }
}
