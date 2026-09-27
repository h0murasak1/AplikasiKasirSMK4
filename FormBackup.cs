using Microsoft.Data.Sqlite;

namespace AplikasiKasirSMK4
{
    /// <summary>
    /// Form untuk memindahkan atau memulihkan seluruh data aplikasi
    /// menggunakan flashdisk atau folder lain.
    /// </summary>
    /// <remarks>
    /// Satu file kasir.db berisi seluruh data. Fitur ini menyalin
    /// file tersebut ke tujuan yang dipilih pengguna, dan
    /// mengembalikannya dari sana. Checksum SHA-256 dihitung sebelum
    /// dan sesudah penyalinan untuk memastikan tidak ada bit yang rusak.
    /// <para>
    /// Koneksi wajib ditutup sebelum penyalinan agar file tidak terkunci.
    /// Karena Pooling=False, menutup semua SqliteConnection cukup.
    /// </para>
    /// </remarks>
    public class FormBackup : Form
    {
        // ─── Kontrol ────────────────────────────────────────────────
        private readonly Label  lblPathDb    = new();
        private readonly Label  lblUkuran    = new();
        private readonly Button btnPilihTujuan = new();
        private readonly TextBox txtTujuan   = new();
        private readonly Button btnBackup    = new();
        private readonly Button btnRestore   = new();
        private readonly RichTextBox rtbLog  = new();
        private readonly ProgressBar progress = new();

        private readonly Koneksi _koneksi = new();

        public FormBackup()
        {
            Text            = "Backup / Restore Data";
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            MinimumSize     = new Size(620, 500);
            ClientSize      = new Size(640, 520);
            Font            = new Font("Segoe UI", 9F);

            BangunTampilan();
            Load += FormBackup_Load;
        }

        // ─── BANGUN TAMPILAN ─────────────────────────────────────────

        private void BangunTampilan()
        {
            var layout = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                ColumnCount = 1,
                RowCount    = 8,
                Padding     = new Padding(14)
            };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));   // info db
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));   // ukuran
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));   // label tujuan
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F)); // input tujuan
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F)); // tombol
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F)); // progress
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F)); // label log
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // log area

            // Informasi database aktif
            lblPathDb.Dock     = DockStyle.Fill;
            lblPathDb.AutoSize = true;
            lblPathDb.Font     = new Font("Segoe UI", 8.5F);
            lblPathDb.ForeColor = Color.FromArgb(60, 60, 60);
            layout.Controls.Add(lblPathDb, 0, 0);

            lblUkuran.Dock     = DockStyle.Fill;
            lblUkuran.AutoSize = true;
            lblUkuran.Font     = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            lblUkuran.ForeColor = Color.FromArgb(39, 174, 96);
            layout.Controls.Add(lblUkuran, 0, 1);

            // Folder tujuan
            layout.Controls.Add(new Label { Text = "Folder atau Drive Tujuan:", Dock = DockStyle.Fill, AutoSize = true }, 0, 2);

            var panelPath = new Panel { Dock = DockStyle.Fill };
            txtTujuan.Width = 440;
            txtTujuan.ReadOnly = true;
            txtTujuan.BackColor = Color.FromArgb(245, 245, 245);
            txtTujuan.Location = new Point(0, 4);

            KonfigTombol(btnPilihTujuan, "📁 Pilih Folder...", Color.FromArgb(52, 152, 219));
            btnPilihTujuan.Location = new Point(448, 2);
            btnPilihTujuan.Width    = 150;

            panelPath.Controls.AddRange(new Control[] { txtTujuan, btnPilihTujuan });
            layout.Controls.Add(panelPath, 0, 3);

            // Tombol backup dan restore
            var panelTombol = new FlowLayoutPanel
            {
                Dock          = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                Padding       = new Padding(0, 4, 0, 0)
            };
            KonfigTombol(btnBackup,  "📤 Backup ke Tujuan",     Color.FromArgb(39, 174, 96));
            KonfigTombol(btnRestore, "📥 Restore dari Tujuan",  Color.FromArgb(231, 76, 60));
            btnBackup.Width  = 190;
            btnRestore.Width = 190;
            panelTombol.Controls.AddRange(new Control[] { btnBackup, btnRestore });
            layout.Controls.Add(panelTombol, 0, 4);

            // Progress
            progress.Dock  = DockStyle.Fill;
            progress.Style = ProgressBarStyle.Marquee;
            progress.Visible = false;
            layout.Controls.Add(progress, 0, 5);

            // Log
            layout.Controls.Add(new Label { Text = "Log:", Dock = DockStyle.Fill, AutoSize = true }, 0, 6);

            rtbLog.Dock      = DockStyle.Fill;
            rtbLog.ReadOnly  = true;
            rtbLog.BackColor = Color.FromArgb(30, 30, 30);
            rtbLog.ForeColor = Color.FromArgb(0, 220, 120);
            rtbLog.Font      = new Font("Consolas", 8.5F);
            rtbLog.BorderStyle = BorderStyle.None;
            layout.Controls.Add(rtbLog, 0, 7);

            Controls.Add(layout);

            btnPilihTujuan.Click += BtnPilihTujuan_Click;
            btnBackup.Click      += BtnBackup_Click;
            btnRestore.Click     += BtnRestore_Click;
        }

        private static void KonfigTombol(Button btn, string teks, Color warna)
        {
            btn.Text      = teks;
            btn.Width     = 160;
            btn.Height    = 32;
            btn.FlatStyle = FlatStyle.Flat;
            btn.BackColor = warna;
            btn.ForeColor = Color.White;
            btn.Cursor    = Cursors.Hand;
            btn.FlatAppearance.BorderSize = 0;
            btn.Margin    = new Padding(0, 0, 8, 0);
        }

        // ─── LOAD ────────────────────────────────────────────────────

        private void FormBackup_Load(object? sender, EventArgs e)
        {
            if (!Session.IsAdmin)
            {
                MessageBox.Show("Hanya Admin yang dapat mengakses halaman ini.",
                    "Akses Ditolak", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }

            string pathDb = _koneksi.PathDatabase;
            lblPathDb.Text = "Database aktif: " + pathDb;

            try
            {
                var info = new FileInfo(pathDb);
                if (info.Exists)
                {
                    double mb = info.Length / 1024.0 / 1024.0;
                    lblUkuran.Text = $"Ukuran file: {mb:F2} MB  ({info.Length:N0} bytes)";
                }
            }
            catch { /* tidak kritis */ }

            Log("Siap. Pilih folder tujuan, lalu klik Backup atau Restore.");
        }

        // ─── PILIH FOLDER ────────────────────────────────────────────

        private void BtnPilihTujuan_Click(object? sender, EventArgs e)
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "Pilih folder tujuan backup (bisa flashdisk, drive lain, dll)",
                UseDescriptionForTitle = true,
                ShowNewFolderButton = true
            };
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                txtTujuan.Text = dialog.SelectedPath;
            }
        }

        // ─── BACKUP ──────────────────────────────────────────────────

        private async void BtnBackup_Click(object? sender, EventArgs e)
        {
            string folder = txtTujuan.Text.Trim();
            if (folder.Length == 0)
            {
                MessageBox.Show("Pilih folder tujuan terlebih dahulu.",
                    "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string pathDb = _koneksi.PathDatabase;
            if (!File.Exists(pathDb))
            {
                MessageBox.Show("File database tidak ditemukan:\n" + pathDb,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string namaBerkas = Path.GetFileNameWithoutExtension(pathDb)
                              + "_backup_"
                              + DateTime.Now.ToString("yyyyMMdd_HHmmss")
                              + Path.GetExtension(pathDb);
            string tujuan = Path.Combine(folder, namaBerkas);

            var konfirmasi = MessageBox.Show(
                $"Backup ke:\n{tujuan}\n\nLanjutkan?",
                "Konfirmasi Backup", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (konfirmasi != DialogResult.Yes) return;

            AktifkanProgress(true);
            Log($"Memulai backup → {tujuan}");

            try
            {
                // Hitung checksum sumber
                string hashSumber = await HitungHashAsync(pathDb);
                Log($"Hash sumber : {hashSumber}");

                // Salin file
                await Task.Run(() => File.Copy(pathDb, tujuan, overwrite: false));

                // Verifikasi
                string hashTujuan = await HitungHashAsync(tujuan);
                Log($"Hash tujuan : {hashTujuan}");

                if (hashSumber != hashTujuan)
                {
                    File.Delete(tujuan);
                    throw new Exception("Verifikasi checksum GAGAL. File tujuan telah dihapus.");
                }

                long ukuran = new FileInfo(tujuan).Length;
                Log($"✔ Backup berhasil! Ukuran: {ukuran:N0} bytes");
                Log($"  Disimpan di: {tujuan}");

                MessageBox.Show(
                    $"Backup berhasil!\n\nFile: {namaBerkas}\nHash: {hashSumber[..16]}...",
                    "Berhasil", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Log($"✘ Error: {ex.Message}");
                MessageBox.Show("Backup gagal:\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                AktifkanProgress(false);
            }
        }

        // ─── RESTORE ─────────────────────────────────────────────────

        private async void BtnRestore_Click(object? sender, EventArgs e)
        {
            string folder = txtTujuan.Text.Trim();
            if (folder.Length == 0)
            {
                MessageBox.Show("Pilih folder sumber (tempat file backup berada) terlebih dahulu.",
                    "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Biarkan pengguna memilih file .db spesifik
            using var dialog = new OpenFileDialog
            {
                Title            = "Pilih file backup database",
                InitialDirectory = folder,
                Filter           = "Database SQLite|*.db|Semua file|*.*",
                CheckFileExists  = true
            };
            if (dialog.ShowDialog(this) != DialogResult.OK) return;

            string sumber = dialog.FileName;
            string pathDb = _koneksi.PathDatabase;

            var konfirmasi = MessageBox.Show(
                $"⚠ PERHATIAN\n\nRestore akan MENGGANTIKAN seluruh data aktif dengan isi file:\n{sumber}\n\n"
                + "Data yang ada sekarang akan HILANG permanen.\n\nYakin lanjutkan?",
                "Konfirmasi Restore", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (konfirmasi != DialogResult.Yes) return;

            AktifkanProgress(true);
            Log($"Memulai restore ← {sumber}");

            try
            {
                string hashSumber = await HitungHashAsync(sumber);
                Log($"Hash sumber : {hashSumber}");

                // Backup otomatis database aktif sebelum ditimpa
                string backup = pathDb + ".bak_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
                await Task.Run(() => File.Copy(pathDb, backup, overwrite: true));
                Log($"Backup otomatis aktif → {backup}");

                // Timpa database aktif
                await Task.Run(() => File.Copy(sumber, pathDb, overwrite: true));

                string hashTujuan = await HitungHashAsync(pathDb);
                Log($"Hash tujuan : {hashTujuan}");

                if (hashSumber != hashTujuan)
                {
                    // Pulihkan dari backup otomatis
                    File.Copy(backup, pathDb, overwrite: true);
                    throw new Exception("Verifikasi checksum GAGAL. Database aktif dipulihkan dari backup otomatis.");
                }

                Log("✔ Restore berhasil! Aplikasi perlu dijalankan ulang.");
                MessageBox.Show(
                    "Restore berhasil!\n\nTutup dan buka kembali aplikasi untuk memuat data baru.",
                    "Berhasil", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Log($"✘ Error: {ex.Message}");
                MessageBox.Show("Restore gagal:\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                AktifkanProgress(false);
            }
        }

        // ─── HELPER ─────────────────────────────────────────────────

        private static async Task<string> HitungHashAsync(string path)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            byte[] hash = await Task.Run(() => sha.ComputeHash(stream));
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        private void Log(string pesan)
        {
            if (InvokeRequired) { Invoke(() => Log(pesan)); return; }
            rtbLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {pesan}{Environment.NewLine}");
            rtbLog.ScrollToCaret();
        }

        private void AktifkanProgress(bool aktif)
        {
            if (InvokeRequired) { Invoke(() => AktifkanProgress(aktif)); return; }
            progress.Visible = aktif;
            btnBackup.Enabled  = !aktif;
            btnRestore.Enabled = !aktif;
        }
    }
}
