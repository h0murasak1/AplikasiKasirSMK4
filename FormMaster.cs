using System.Data;
using Microsoft.Data.Sqlite;

namespace AplikasiKasirSMK4
{
    /// <summary>
    /// Form generik untuk seluruh tabel master sederhana.
    /// </summary>
    /// <remarks>
    /// Empat master, yaitu jenis barang, merek, supplier, dan sales,
    /// dilayani oleh satu form ini. Yang membedakan hanya isi form,
    /// dan itu semuanya berasal dari MetadataMaster.
    /// <para>
    /// Kenapa tidak empat form terpisah? Karena keempatnya tidak punya
    /// satu pun perbedaan yang berarti: sama-sama daftar sederhana
    /// dengan kolom nama, keterangan, dan penanda aktif. Kalau ditulis
    /// terpisah, perbaikan di satu tempat harus diulang di tiga tempat
    /// lain, dan cepat atau lambat satu dari empatnya tertinggal.
    /// </para>
    /// <para>
    /// Bentuknya dibuat langsung di kode, bukan lewat desainer. Isinya
    /// bergantung pada metadata, jadi menulisnya sebagai desainer akan
    /// berarti control yang selalu sama untuk keempat master, dan kolom
    /// tambahan seperti komisi sales tidak akan pernah muncul.
    /// </para>
    /// </remarks>
    public partial class FormMaster : Form
    {
        private readonly Koneksi _koneksi = new();
        private readonly MetadataMaster _meta;

        private TextBox txtNama = new();
        private TextBox txtKode = new();
        private TextBox txtCari = new();
        private DataGridView dgv = new();
        private CheckBox chkTampilkanNonaktif = new();
        private CheckBox chkAktif = new();

        /// <summary>Control isian yang dibuat dari metadata, kolom per kolom.</summary>
        private readonly Dictionary<string, Control> _isian = new();

        private int _idTerpilih;
        private bool _sedangMuat;

        /// <summary>
        /// Membuka form untuk master tertentu.
        /// </summary>
        /// <remarks>
        /// Pemanggilnya adalah statis Untuk, bukan kode yang memanggil
        /// constructor ini langsung. Metadata tetap internal supaya
        /// tidak bisa dibuat di luar aplikasi, dan nama tabel yang
        /// diterima sudah divalidasi terhadap daftar master.
        /// </remarks>
        internal FormMaster(MetadataMaster meta)
        {
            _meta = meta ?? throw new ArgumentNullException(nameof(meta));

            Text = _meta.Judul;
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(720, 560);
            Size = new Size(780, 640);
            Font = new Font("Segoe UI", 9F);

            BangunTampilan();
        }

        /// <summary>
        /// Membuka form master berdasarkan nama tabelnya.
        /// </summary>
        /// <remarks>
        /// Dipakai dari menu, yang hanya menyimpan nama tabel sebagai
        /// teks. Nama yang tidak terdaftar ditolak di sini, bukan
        /// diteruskan ke constructor, supaya tabel yang tidak dikenal
        /// tidak pernah sampai ke query.
        /// </remarks>
        public static FormMaster Untuk(string tabel)
        {
            MetadataMaster? meta = MasterData.Cari(tabel);
            if (meta is null)
            {
                throw new ArgumentException(
                    "Tabel \"" + tabel + "\" bukan master yang terdaftar.",
                    nameof(tabel));
            }

            return new FormMaster(meta);
        }

        /// <summary>
        /// Membuka form untuk master berdasarkan posisinya di Daftar.
        /// </summary>
        /// <remarks>
        /// Dipakai oleh menu yang susunan tombolnya memang sudah
        /// dibuat dari daftar master, jadi tidak perlu menulis nama
        /// tabelnya satu per satu.
        /// </remarks>
        public static FormMaster Untuk(int urutan)
        {
            if (urutan < 0 || urutan >= MasterData.Semua.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(urutan));
            }

            return new FormMaster(MasterData.Semua[urutan]);
        }

        // ------------------------------------------------------------------
        // Susunan tampilan
        // ------------------------------------------------------------------
        private void BangunTampilan()
        {
            var utama = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(12)
            };
            utama.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            utama.RowStyles.Add(new RowStyle(SizeType.AutoSize));   // judul
            utama.RowStyles.Add(new RowStyle(SizeType.AutoSize));   // form isian
            utama.RowStyles.Add(new RowStyle(SizeType.AutoSize));   // tombol
            utama.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // daftar

            Controls.Add(utama);
            utama.Controls.Add(BangunJudul(), 0, 0);
            utama.Controls.Add(BangunPanelIsian(), 0, 1);
            utama.Controls.Add(BangunPanelTombol(), 0, 2);
            utama.Controls.Add(BangunDaftar(), 0, 3);
        }

        private Control BangunJudul()
        {
            var panel = new Panel { Dock = DockStyle.Fill, AutoSize = true };

            var lblJudul = new Label
            {
                Text = _meta.Judul,
                Dock = DockStyle.Top,
                AutoSize = true,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 30, 30)
            };

            var lblKeterangan = new Label
            {
                Text = "Isi daftar, lalu klik Simpan untuk menambah. "
                     + "Baris yang tidak terpakai boleh dinonaktifkan, "
                     + "bukan dihapus, supaya data lama tetap utuh.",
                Dock = DockStyle.Bottom,
                AutoSize = true,
                ForeColor = Color.DimGray
            };

            panel.Controls.Add(lblKeterangan);
            panel.Controls.Add(lblJudul);
            return panel;
        }

        /// <summary>
        /// Membangun panel isian dari metadata.
        /// </summary>
        /// <remarks>
        /// Dua kolom isian selalu ada: kode (hanya kalau tabelnya punya
        /// kolom kode) dan nama. Sisanya diambil dari KolomTambahan.
        /// </remarks>
        private Control BangunPanelIsian()
        {
            var tabel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                ColumnCount = 3,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            tabel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            tabel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tabel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            int baris = 0;

            if (_meta.KolomKode is not null)
            {
                tabel.Controls.Add(MakingLabel(_meta.LabelKode), 0, baris);
                tabel.Controls.Add(txtKode, 1, baris);
                tabel.Controls.Add(MakingLabel(""), 2, baris);
                txtKode.MaxLength = 30;
                baris++;
            }

            tabel.Controls.Add(MakingLabel(_meta.LabelNama), 0, baris);
            tabel.Controls.Add(txtNama, 1, baris);
            tabel.Controls.Add(MakingLabel("wajib"), 2, baris);
            txtNama.MaxLength = 100;
            baris++;

            foreach (KolomMaster kolom in _meta.KolomTambahan)
            {
                tabel.Controls.Add(MakingLabel(kolom.Label), 0, baris);
                tabel.Controls.Add(BuatIsian(kolom), 1, baris);

                string penanda = kolom.Wajib ? "wajib" : "opsional";
                if (kolom.Petunjuk is not null)
                {
                    penanda = kolom.Petunjuk;
                }
                tabel.Controls.Add(MakingLabel(penanda), 2, baris);
                baris++;
            }

            tabel.Controls.Add(MakingLabel(""), 0, baris);
            chkAktif.Text = "Aktif";
            chkAktif.AutoSize = true;
            chkAktif.Checked = true;
            tabel.Controls.Add(chkAktif, 1, baris);

            var bungkus = new Panel { Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(0, 6, 0, 6) };
            bungkus.Controls.Add(tabel);
            return bungkus;
        }

        private static Label MakingLabel(string teks)
        {
            return new Label
            {
                Text = teks,
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                ForeColor = Color.FromArgb(60, 60, 60),
                Margin = new Padding(0, 7, 8, 0)
            };
        }

        /// <summary>
        /// Membuat control isian yang sesuai dengan macam kolomnya.
        /// </summary>
        /// <remarks>
        /// Bedanya cuma satu: kolom teks panjang dapat kotak yang
        /// beberapa baris tinggi, sisanya sebaris. Tidak ada saringan
        /// ketik di sini, dan itu disengaja. Satu-satunya kolom angka
        /// di keempat master adalah komisi sales, dan komisi boleh
        /// pecahan karena CHECK di database juga mengizinkan dua
        /// desimal. Mengunci digit di sini justru akan membuat orang
        /// menulis 2,5 menjadi 25 atau 2 dan kehilangan pecahan.
        /// </remarks>
        private Control BuatIsian(KolomMaster kolom)
        {
            Control kontrol;

            if (kolom.Macam == MacamKolomMaster.TeksPanjang)
            {
                kontrol = new TextBox
                {
                    Multiline = true,
                    ScrollBars = ScrollBars.Vertical,
                    Height = 60,
                    Anchor = AnchorStyles.Left | AnchorStyles.Right
                };
            }
            else
            {
                kontrol = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };
            }

            if (kolom.PanjangMaks is int maks)
            {
                ((TextBox)kontrol).MaxLength = maks;
            }

            kontrol.Tag = kolom.Kolom;
            _isian[kolom.Kolom] = kontrol;
            return kontrol;
        }

        private Control BangunPanelTombol()
        {
            var panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0, 4, 0, 4)
            };

            panel.Controls.Add(BuatTombol("Simpan", btnSimpan_Click, Color.FromArgb(0, 122, 204)));
            panel.Controls.Add(BuatTombol("Baru", btnBaru_Click, Color.FromArgb(90, 90, 90)));
            panel.Controls.Add(BuatTombol("Perbarui", btnPerbarui_Click, Color.FromArgb(0, 122, 204)));
            panel.Controls.Add(BuatTombol("Aktifkan", btnAktifkan_Click, Color.FromArgb(38, 140, 78)));
            panel.Controls.Add(BuatTombol("Nonaktifkan", btnNonaktifkan_Click, Color.FromArgb(180, 70, 40)));
            panel.Controls.Add(BuatTombol("Tutup", (_, _) => Close(), Color.FromArgb(90, 90, 90)));

            return panel;
        }

        private static Button BuatTombol(string teks, EventHandler aksi, Color warna)
        {
            var tombol = new Button
            {
                Text = teks,
                AutoSize = true,
                Height = 34,
                Padding = new Padding(14, 0, 14, 0),
                FlatStyle = FlatStyle.Flat,
                BackColor = warna,
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 8, 0)
            };

            tombol.FlatAppearance.BorderSize = 0;
            tombol.FlatAppearance.MouseOverBackColor = ControlPaint.Light(warna, 0.15f);
            tombol.Click += aksi;
            return tombol;
        }

        private Control BangunDaftar()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 8, 0, 0) };

            var cariPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight
            };

            cariPanel.Controls.Add(new Label
            {
                Text = "Cari:",
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(0, 7, 6, 0)
            });

            txtCari.Width = 220;
            txtCari.TextChanged += (_, _) => TampilData();
            cariPanel.Controls.Add(txtCari);

            chkTampilkanNonaktif.Text = "Tampilkan yang nonaktif";
            chkTampilkanNonaktif.AutoSize = true;
            chkTampilkanNonaktif.Margin = new Padding(12, 7, 0, 0);
            chkTampilkanNonaktif.CheckedChanged += (_, _) => TampilData();
            cariPanel.Controls.Add(chkTampilkanNonaktif);

            dgv.Dock = DockStyle.Fill;
            dgv.ReadOnly = true;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.RowHeadersVisible = false;
            dgv.SelectionChanged += (_, _) => BarisDipilih();
            dgv.CellDoubleClick += (_, _) => MuatKeForm();

            panel.Controls.Add(dgv);
            panel.Controls.Add(cariPanel);
            return panel;
        }

        // ------------------------------------------------------------------
        // Pembacaan daftar
        // ------------------------------------------------------------------
        private void TampilData()
        {
            if (_sedangMuat)
            {
                return;
            }

            try
            {
                using SqliteConnection conn = _koneksi.GetConn();

                string sql = SqlMaster.Daftar(
                    _meta, chkTampilkanNonaktif.Checked, txtCari.Text);

                using SqliteCommand cmd = new(sql, conn);

                string? cari = SqlMaster.NilaiCari(txtCari.Text);
                if (cari is not null)
                {
                    cmd.Parameters.AddWithValue("@cari", cari);
                }

                DataTable dt = QueryHelper.IsiTabel(cmd);

                _sedangMuat = true;
                dgv.DataSource = dt;

                // Kolom penanda aktif disembunyikan dari daftar. Yang
                // menentukan tampilan bukan kolom ini, tapi filter
                // "tampilkan yang nonaktif" di atas.
                if (dgv.Columns["Aktif"] is DataGridViewColumn kolomAktif)
                {
                    kolomAktif.Visible = false;
                }

                // Assign DataSource memicu SelectionChanged, dan event
                // itu diabaikan selama _sedangMuat menyala. Kalau baris
                // pertama tidak dibaca di sini, daftar akan terlihat
                // punya baris terpilih, tapi _idTerpilih masih nol, dan
                // tombol Perbarui akan bilang "pilih dulu" padahal sudah
                // ada yang terpilih. Membacanya langsung menutup celah itu.
                AmbilIdBarisTerpilih();

                _sedangMuat = false;
            }
            catch (Exception ex)
            {
                _sedangMuat = false;
                MessageBox.Show(
                    "Gagal memuat daftar: " + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // ------------------------------------------------------------------
        // Pemilihan baris
        // ------------------------------------------------------------------
        private void BarisDipilih()
        {
            if (_sedangMuat)
            {
                return;
            }

            AmbilIdBarisTerpilih();
        }

        /// <summary>
        /// Membaca id dari baris yang sedang terpilih di daftar.
        /// </summary>
        /// <remarks>
        /// Mengembalikan nilai kosong kalau daftar kosong atau baris
        /// terpilih tidak terikat ke data. Pemanggil yang butuh
        /// memberitahu kasir soal keadaan itu tetap memanggil
        /// AdaBarisTerpilih.
        /// </remarks>
        private void AmbilIdBarisTerpilih()
        {
            _idTerpilih = 0;

            if (dgv.CurrentRow is null || dgv.RowCount == 0)
            {
                return;
            }

            if (dgv.CurrentRow.DataBoundItem is not DataRowView tampilan)
            {
                return;
            }

            _idTerpilih = Convert.ToInt32(tampilan["Id"]);
        }

        private bool AdaBarisTerpilih()
        {
            if (_idTerpilih > 0)
            {
                return true;
            }

            MessageBox.Show(
                "Pilih dulu satu baris di daftar.",
                "Peringatan",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return false;
        }

        private void MuatKeForm()
        {
            if (!AdaBarisTerpilih())
            {
                return;
            }

            try
            {
                using SqliteConnection conn = _koneksi.GetConn();

                string sql = SqlMaster.SatuBaris(_meta);
                using SqliteCommand cmd = new(sql, conn);
                cmd.Parameters.AddWithValue("@id", _idTerpilih);

                using SqliteDataReader r = cmd.ExecuteReader();
                if (!r.Read())
                {
                    return;
                }

                txtNama.Text = r[_meta.KolomNama].ToString() ?? string.Empty;
                if (_meta.KolomKode is not null)
                {
                    txtKode.Text = r[_meta.KolomKode].ToString() ?? string.Empty;
                }

                foreach (KolomMaster kolom in _meta.KolomTambahan)
                {
                    if (!_isian.TryGetValue(kolom.Kolom, out Control? kontrol))
                    {
                        continue;
                    }

                    kontrol.Text = FormatNilaiKolom(r[kolom.Kolom]);
                }

                chkAktif.Checked = Convert.ToInt64(r[_meta.KolomAktif]) == 1;
                Text = _meta.Judul + " - " + txtNama.Text;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat data: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Mengubah nilai database menjadi teks yang enak dibaca.
        /// </summary>
        /// <remarks>
        /// Angka pecahan ditulis dengan pemisah ribuan supaya
        /// persentase komisi tampil sebagai "5" dan bukan "5,00000000".
        /// </remarks>
        private static string FormatNilaiKolom(object? nilai)
        {
            if (nilai is null || nilai is DBNull)
            {
                return string.Empty;
            }

            if (nilai is long or int or short or byte)
            {
                return Convert.ToInt64(nilai).ToString();
            }

            if (nilai is double or float or decimal)
            {
                decimal angka = Convert.ToDecimal(nilai);
                return angka == decimal.Truncate(angka)
                    ? ((long)angka).ToString()
                    : angka.ToString("0.##");
            }

            return nilai.ToString() ?? string.Empty;
        }

        // ------------------------------------------------------------------
        // Penyimpanan
        // ------------------------------------------------------------------

        /// <summary>
        /// Membaca semua isian, memeriksa kelayakannya, lalu menyimpan.
        /// </summary>
        /// <remarks>
        /// Pemeriksaan dilakukan di sini, bukan menyerahkannya ke
        /// database. Alasannya, pesan dari database berbahasa SQLite dan
        /// menyebut nomor baris, yang tidak berarti apa-apa bagi kasir.
        /// Aturan yang tetap dijaga database tetap ada sebagai jaring
        /// pengaman terakhir.
        /// </remarks>
        /// <returns>True kalau data berhasil disimpan.</returns>
        private bool Simpan(bool adalahBaru)
        {
            if (!KumpulkanNilai(out Dictionary<string, object?> nilai, out string? galat))
            {
                MessageBox.Show(galat!, "Peringatan",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            try
            {
                using SqliteConnection conn = _koneksi.GetConn();

                if (adalahBaru)
                {
                    if (CariIdDenganNama(conn, nilai) > 0)
                    {
                        MessageBox.Show(
                            _meta.LabelNama + " \"" + nilai[_meta.KolomNama]
                            + "\" sudah dipakai.\nGunakan nama lain, atau "
                            + "aktifkan kembali baris yang nonaktif.",
                            "Peringatan",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }

                    SimpanBaru(conn, nilai);
                }
                else
                {
                    Perbarui(conn, nilai);
                }

                TampilData();
                BersihkanForm();
                return true;
            }
            catch (SqliteException ex) when (SqliteError.AdalahUnik(ex))
            {
                MessageBox.Show(
                    _meta.LabelNama + " sudah dipakai data lain.", "Peringatan",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal menyimpan data: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Mengumpulkan nilai dari seluruh isian lalu memeriksanya.
        /// </summary>
        private bool KumpulkanNilai(out Dictionary<string, object?> nilai, out string? galat)
        {
            nilai = new Dictionary<string, object?>();
            galat = null;

            string nama = txtNama.Text.Trim();
            if (nama.Length == 0)
            {
                galat = _meta.LabelNama + " tidak boleh kosong!";
                txtNama.Focus();
                return false;
            }

            nilai[_meta.KolomNama] = nama;

            if (_meta.KolomKode is not null)
            {
                nilai[_meta.KolomKode] = txtKode.Text.Trim();
            }

            foreach (KolomMaster kolom in _meta.KolomTambahan)
            {
                string teks = _isian.TryGetValue(kolom.Kolom, out Control? k)
                    ? k.Text
                    : string.Empty;

                object? hasil = kolom.NilaiDari(teks);

                if (kolom.Wajib && hasil is null)
                {
                    galat = kolom.Label + " tidak boleh kosong!";
                    _isian[kolom.Kolom].Focus();
                    return false;
                }

                if (hasil is null && kolom.Macam == MacamKolomMaster.Desimal)
                {
                    // Teks yang tidak bisa dibaca sebagai angka. Nilainya
                    // sengaja ditolak apa adanya, bukan diubah diam-diam
                    // menjadi nol, karena nol adalah nilai yang sah.
                    galat = kolom.Label + " harus berupa angka, bukan \"" + teks.Trim() + "\".";
                    return false;
                }

                if (hasil is decimal angka)
                {
                    if (kolom.Minimum is decimal min && angka < min)
                    {
                        galat = kolom.Label + " tidak boleh kurang dari " + min + ".";
                        return false;
                    }

                    if (kolom.Maksimum is decimal maks && angka > maks)
                    {
                        galat = kolom.Label + " tidak boleh lebih dari " + maks + ".";
                        return false;
                    }
                }

                nilai[kolom.Kolom] = hasil;
            }

            return true;
        }

        private void SimpanBaru(SqliteConnection conn, Dictionary<string, object?> nilai)
        {
            using SqliteCommand cmd = new(SqlMaster.Sisip(_meta, nilai.Keys), conn);
            SqlMaster.PasangNilai(cmd, nilai);
            cmd.ExecuteNonQuery();
        }

        private void Perbarui(SqliteConnection conn, Dictionary<string, object?> nilai)
        {
            using SqliteCommand cmd = new(SqlMaster.Perbarui(_meta), conn);
            SqlMaster.PasangNilai(cmd, nilai);
            cmd.Parameters.AddWithValue("@aktif", chkAktif.Checked ? 1 : 0);
            cmd.Parameters.AddWithValue("@id", _idTerpilih);

            if (cmd.ExecuteNonQuery() == 0)
            {
                MessageBox.Show(
                    "Data yang akan diperbarui sudah tidak ada. "
                    + "Mungkin sudah dihapus di jendela lain.",
                    "Peringatan",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Mencari id baris dengan nama yang sama.
        /// </summary>
        private long CariIdDenganNama(SqliteConnection conn, Dictionary<string, object?> nilai)
        {
            using SqliteCommand cmd = new(SqlMaster.CariIdDenganNama(_meta), conn);
            cmd.Parameters.AddWithValue("@nama", nilai[_meta.KolomNama]);

            object? hasil = cmd.ExecuteScalar();
            return hasil is null || hasil is DBNull ? 0 : Convert.ToInt64(hasil);
        }

        // ------------------------------------------------------------------
        // Aksi tombol
        // ------------------------------------------------------------------
        private void btnSimpan_Click(object? sender, EventArgs e)
        {
            Simpan(adalahBaru: true);
        }

        private void btnPerbarui_Click(object? sender, EventArgs e)
        {
            if (!AdaBarisTerpilih())
            {
                return;
            }

            if (Simpan(adalahBaru: false))
            {
                MessageBox.Show("Data berhasil diperbarui.", "Sukses",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnBaru_Click(object? sender, EventArgs e)
        {
            BersihkanForm();
        }

        private void btnNonaktifkan_Click(object? sender, EventArgs e)
        {
            UbahAktif(false);
        }

        private void btnAktifkan_Click(object? sender, EventArgs e)
        {
            UbahAktif(true);
        }

        /// <summary>
        /// Mengaktifkan atau menonaktifkan baris yang dipilih.
        /// </summary>
        /// <remarks>
        /// Tidak ada tombol hapus sama sekali. Master yang sudah
        /// dipakai transaksi atau barang tidak boleh hilang, karena
        /// nota lama dan kartu stok harus tetap bisa dibaca. Menonaktifkan
        /// menyembunyikannya dari daftar tanpa menghapus apa pun.
        /// </remarks>
        private void UbahAktif(bool aktif)
        {
            if (!AdaBarisTerpilih())
            {
                return;
            }

            try
            {
                using SqliteConnection conn = _koneksi.GetConn();

                using SqliteCommand cmd = new(SqlMaster.UbahAktif(_meta), conn);
                cmd.Parameters.AddWithValue("@aktif", aktif ? 1 : 0);
                cmd.Parameters.AddWithValue("@id", _idTerpilih);
                cmd.ExecuteNonQuery();

                TampilData();
                BersihkanForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal mengubah status: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ------------------------------------------------------------------
        // Pembersihan form
        // ------------------------------------------------------------------
        private void BersihkanForm()
        {
            _idTerpilih = 0;

            // _sedangMuat dipakai supaya event TextChanged yang dipicu
            // pengosongan kotak tidak ikut memicu pemuatan ulang daftar.
            _sedangMuat = true;
            try
            {
                txtNama.Text = string.Empty;
                txtKode.Text = string.Empty;

                foreach (Control kontrol in _isian.Values)
                {
                    kontrol.Text = string.Empty;
                }

                chkAktif.Checked = true;
                dgv.ClearSelection();
                Text = _meta.Judul;
            }
            finally
            {
                _sedangMuat = false;
            }

            txtNama.Focus();
        }

        // ------------------------------------------------------------------
        // Siklus hidup
        // ------------------------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Guard: seluruh master hanya boleh dikelola Admin. Data
            // master dipakai saat transaksi, jadi kasir tidak boleh
            // mengubahnya di belakang layar.
            if (!Session.IsAdmin)
            {
                MessageBox.Show(
                    "Hanya akun Admin yang dapat mengelola data master.",
                    "Akses Ditolak",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                Close();
                return;
            }

            TampilData();
        }
    }
}
