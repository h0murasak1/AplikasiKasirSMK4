using System.Drawing.Printing;
using System.Text;
using System.Text.Json;
using Microsoft.Data.Sqlite;

namespace AplikasiKasirSMK4
{
    public class StrukItem
    {
        public string Kode { get; set; } = string.Empty;
        public string Nama { get; set; } = string.Empty;
        public decimal Qty { get; set; }
        public decimal HargaSatuan { get; set; }
        public decimal Subtotal { get; set; }
        public bool IsGrosir { get; set; }
    }

    public class StrukData
    {
        public string NoNota { get; set; } = string.Empty;
        public DateTime Waktu { get; set; } = DateTime.Now;
        public string NamaKasir { get; set; } = string.Empty;
        public string? NamaMember { get; set; }
        public string? NamaSales { get; set; }
        public string MetodeBayar { get; set; } = "Tunai";
        public List<StrukItem> Items { get; set; } = new();
        public decimal SubtotalKotor { get; set; }
        public decimal DiskonMember { get; set; }
        public decimal DiskonTambahan { get; set; }
        public decimal DiskonTotal => DiskonMember + DiskonTambahan;
        public decimal NilaiPpn { get; set; }
        public decimal PpnPersen { get; set; }
        public decimal TotalBayar { get; set; }
        public decimal UangDiterima { get; set; }
        public decimal Kembalian { get; set; }
        public int PoinDidapat { get; set; }
    }

    /// <summary>
    /// Layanan pencetakan struk dan kontrol Cash Drawer untuk Printer POS (khususnya EPSON TM-U220D).
    /// </summary>
    public static class PosPrinterService
    {
        private const string ConfigFileName = "printer_config.json";
        private const int MaxCols = 40; // Standar Font A Epson TM-U220D (40 kolom)

        private static string? _cachedPrinterName;

        /// <summary>
        /// Mengambil nama printer yang dipilih. Jika belum ada, otomatis mendeteksi printer Epson / POS / Default.
        /// </summary>
        public static string AmbilNamaPrinter()
        {
            if (!string.IsNullOrWhiteSpace(_cachedPrinterName))
            {
                return _cachedPrinterName;
            }

            string pathConfig = Path.Combine(AppContext.BaseDirectory, ConfigFileName);
            if (File.Exists(pathConfig))
            {
                try
                {
                    string json = File.ReadAllText(pathConfig);
                    using JsonDocument doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("PrinterName", out JsonElement p))
                    {
                        string? name = p.GetString();
                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            _cachedPrinterName = name;
                            return _cachedPrinterName;
                        }
                    }
                }
                catch { /* Abaikan jika file konfigurasi rusak */ }
            }

            // Auto-detect dari installed printers
            string? printerDitemukan = DeteksiPrinterPosOtomatis();
            _cachedPrinterName = printerDitemukan ?? AmbilDefaultPrinter();
            return _cachedPrinterName;
        }

        /// <summary>
        /// Menyimpan pilihan printer ke file konfigurasi lokal.
        /// </summary>
        public static void SimpanNamaPrinter(string printerName)
        {
            _cachedPrinterName = printerName;
            try
            {
                string pathConfig = Path.Combine(AppContext.BaseDirectory, ConfigFileName);
                var obj = new { PrinterName = printerName };
                string json = JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(pathConfig, json, Encoding.UTF8);
            }
            catch { /* Abaikan jika gagal menulis disk */ }
        }

        /// <summary>
        /// Mengambil daftar semua printer yang terpasang di Windows.
        /// </summary>
        public static List<string> AmbilDaftarPrinterTerpasang()
        {
            var list = new List<string>();
            foreach (string printer in PrinterSettings.InstalledPrinters)
            {
                list.Add(printer);
            }
            return list;
        }

        private static string? DeteksiPrinterPosOtomatis()
        {
            var printers = AmbilDaftarPrinterTerpasang();
            // 1. Cari yang mengandung TM-U220 / TM-U220D
            var tm = printers.FirstOrDefault(p => p.Contains("TM-U220", StringComparison.OrdinalIgnoreCase));
            if (tm != null) return tm;

            // 2. Cari yang mengandung EPSON / POS / RECEIPT
            var pos = printers.FirstOrDefault(p =>
                p.Contains("EPSON", StringComparison.OrdinalIgnoreCase) ||
                p.Contains("POS", StringComparison.OrdinalIgnoreCase) ||
                p.Contains("RECEIPT", StringComparison.OrdinalIgnoreCase) ||
                p.Contains("STRUK", StringComparison.OrdinalIgnoreCase));
            if (pos != null) return pos;

            return null;
        }

        private static string AmbilDefaultPrinter()
        {
            try
            {
                PrinterSettings settings = new();
                return settings.PrinterName ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Mengirim perintah ESC/POS untuk membuka Cash Drawer (RJ11) via printer.
        /// </summary>
        public static bool BukaCashDrawer(string? printerName, out string error)
        {
            string target = printerName ?? AmbilNamaPrinter();
            if (string.IsNullOrWhiteSpace(target))
            {
                error = "Printer belum ditentukan atau tidak ditemukan di sistem.";
                return false;
            }

            // Perintah ESC p (Pulse to Cash Drawer)
            // Pin 2 (Drawer 1): 27, 112, 0, 25, 250 (0x1B, 0x70, 0x00, 0x19, 0xFA)
            // Pin 5 (Drawer 2): 27, 112, 1, 25, 250 (0x1B, 0x70, 0x01, 0x19, 0xFA)
            // ESC < (1B 3C)
            // BEL (0x07)
            byte[] cmdBukaDrawer = new byte[]
            {
                0x1B, 0x70, 0x00, 0x19, 0xFA, // Kick Pin 2
                0x1B, 0x70, 0x01, 0x19, 0xFA, // Kick Pin 5
                0x1B, 0x3C,                   // Open Drawer alt
                0x07                          // Bell
            };

            return RawPrinterHelper.SendBytesToPrinter(target, cmdBukaDrawer, out error);
        }

        /// <summary>
        /// Mencetak struk transaksi ke printer POS dan otomatis membuka Cash Drawer.
        /// </summary>
        public static bool CetakStrukDanBukaDrawer(StrukData data, string? printerName, out string error)
        {
            string target = printerName ?? AmbilNamaPrinter();
            if (string.IsNullOrWhiteSpace(target))
            {
                error = "Printer belum ditentukan atau tidak ditemukan.";
                return false;
            }

            // 1. Buka Drawer terlebih dahulu
            BukaCashDrawer(target, out _);

            // 2. Baca Profil Toko
            string namaToko = "SMK NEGERI 4";
            string alamat = "";
            string telepon = "";
            string catatanStruk = "Terima kasih atas kunjungan Anda!";

            try
            {
                using var conn = new Koneksi().GetConn();
                using var cmd = new SqliteCommand("SELECT nama_toko, alamat, telepon, catatan_struk FROM tb_profil_toko WHERE id_toko = 1", conn);
                using var r = cmd.ExecuteReader();
                if (r.Read())
                {
                    string nt = r["nama_toko"]?.ToString() ?? "";
                    if (!string.IsNullOrWhiteSpace(nt)) namaToko = nt;
                    alamat = r["alamat"]?.ToString() ?? "";
                    telepon = r["telepon"]?.ToString() ?? "";
                    string cs = r["catatan_struk"]?.ToString() ?? "";
                    if (!string.IsNullOrWhiteSpace(cs)) catatanStruk = cs;
                }
            }
            catch { /* Gunakan nilai default jika error membaca database */ }

            // 3. Susun data ESC/POS untuk Epson TM-U220D
            byte[] bytesStruk = FormatStrukEscPos(data, namaToko, alamat, telepon, catatanStruk);

            // 4. Kirim ke printer
            return RawPrinterHelper.SendBytesToPrinter(target, bytesStruk, out error);
        }

        /// <summary>
        /// Memformat data struk ke dalam format byte ESC/POS untuk dot-matrix 40 kolom.
        /// </summary>
        private static byte[] FormatStrukEscPos(StrukData data, string namaToko, string alamat, string telepon, string catatanStruk)
        {
            var ms = new MemoryStream();
            using var w = new BinaryWriter(ms, Encoding.ASCII);

            // Inisialisasi printer: ESC @
            w.Write(new byte[] { 0x1B, 0x40 });

            // Font A (Standar): ESC ! 0
            w.Write(new byte[] { 0x1B, 0x21, 0x00 });

            // Rata Tengah: ESC a 1
            w.Write(new byte[] { 0x1B, 0x61, 0x01 });

            // Header Toko (Bold): ESC E 1
            w.Write(new byte[] { 0x1B, 0x45, 0x01 });
            w.Write(Encoding.ASCII.GetBytes(Tengah(namaToko, MaxCols) + "\n"));
            w.Write(new byte[] { 0x1B, 0x45, 0x00 }); // Bold OFF

            if (!string.IsNullOrWhiteSpace(alamat))
            {
                w.Write(Encoding.ASCII.GetBytes(Tengah(alamat, MaxCols) + "\n"));
            }
            if (!string.IsNullOrWhiteSpace(telepon))
            {
                w.Write(Encoding.ASCII.GetBytes(Tengah("Telp: " + telepon, MaxCols) + "\n"));
            }

            // Garis Pembatas
            w.Write(Encoding.ASCII.GetBytes(new string('-', MaxCols) + "\n"));

            // Info Transaksi (Rata Kiri: ESC a 0)
            w.Write(new byte[] { 0x1B, 0x61, 0x00 });
            w.Write(Encoding.ASCII.GetBytes(BarisDuaKolom("No. Nota : " + data.NoNota, data.Waktu.ToString("dd/MM/yyyy HH:mm")) + "\n"));
            w.Write(Encoding.ASCII.GetBytes("Kasir    : " + data.NamaKasir + "\n"));

            if (!string.IsNullOrWhiteSpace(data.NamaMember))
            {
                w.Write(Encoding.ASCII.GetBytes("Member   : " + data.NamaMember + "\n"));
            }
            if (!string.IsNullOrWhiteSpace(data.NamaSales))
            {
                w.Write(Encoding.ASCII.GetBytes("Sales    : " + data.NamaSales + "\n"));
            }

            w.Write(Encoding.ASCII.GetBytes(new string('-', MaxCols) + "\n"));

            // Daftar Item Belanja
            foreach (var item in data.Items)
            {
                // Baris 1: Nama Barang
                w.Write(Encoding.ASCII.GetBytes(item.Nama + "\n"));

                // Baris 2: Qty x Harga   ->   Subtotal
                string qtyHarga = $"  {InputHelper.FormatJumlah(item.Qty)} x {InputHelper.FormatNominal(item.HargaSatuan)}";
                if (item.IsGrosir) qtyHarga += " (Grosir)";
                string subtotal = InputHelper.FormatNominal(item.Subtotal);

                w.Write(Encoding.ASCII.GetBytes(BarisDuaKolom(qtyHarga, subtotal) + "\n"));
            }

            w.Write(Encoding.ASCII.GetBytes(new string('-', MaxCols) + "\n"));

            // Ringkasan Pembayaran
            if (data.DiskonTotal > 0)
            {
                w.Write(Encoding.ASCII.GetBytes(BarisDuaKolom("Subtotal Kotor", "Rp " + InputHelper.FormatNominal(data.SubtotalKotor)) + "\n"));
                if (data.DiskonMember > 0)
                {
                    w.Write(Encoding.ASCII.GetBytes(BarisDuaKolom("Diskon Member", "-Rp " + InputHelper.FormatNominal(data.DiskonMember)) + "\n"));
                }
                if (data.DiskonTambahan > 0)
                {
                    w.Write(Encoding.ASCII.GetBytes(BarisDuaKolom("Diskon Tambahan", "-Rp " + InputHelper.FormatNominal(data.DiskonTambahan)) + "\n"));
                }
            }

            if (data.NilaiPpn > 0)
            {
                w.Write(Encoding.ASCII.GetBytes(BarisDuaKolom($"PPN ({data.PpnPersen:0.##}%)", "Rp " + InputHelper.FormatNominal(data.NilaiPpn)) + "\n"));
            }

            // Total Bayar (Bold)
            w.Write(new byte[] { 0x1B, 0x45, 0x01 }); // Bold ON
            w.Write(Encoding.ASCII.GetBytes(BarisDuaKolom("TOTAL BELANJA", "Rp " + InputHelper.FormatNominal(data.TotalBayar)) + "\n"));
            w.Write(new byte[] { 0x1B, 0x45, 0x00 }); // Bold OFF

            w.Write(Encoding.ASCII.GetBytes(BarisDuaKolom("Metode Bayar", data.MetodeBayar) + "\n"));

            if (data.MetodeBayar.Contains("TEMPO", StringComparison.OrdinalIgnoreCase))
            {
                w.Write(Encoding.ASCII.GetBytes(BarisDuaKolom("Status", "PIUTANG / TEMPO") + "\n"));
            }
            else
            {
                w.Write(Encoding.ASCII.GetBytes(BarisDuaKolom("Bayar", "Rp " + InputHelper.FormatNominal(data.UangDiterima)) + "\n"));
                w.Write(Encoding.ASCII.GetBytes(BarisDuaKolom("Kembalian", "Rp " + InputHelper.FormatNominal(data.Kembalian)) + "\n"));
            }

            if (data.PoinDidapat > 0)
            {
                w.Write(Encoding.ASCII.GetBytes(BarisDuaKolom("Poin Member Diperoleh", $"+{data.PoinDidapat} Poin") + "\n"));
            }

            w.Write(Encoding.ASCII.GetBytes(new string('-', MaxCols) + "\n"));

            // Footer (Rata Tengah: ESC a 1)
            w.Write(new byte[] { 0x1B, 0x61, 0x01 });
            if (!string.IsNullOrWhiteSpace(catatanStruk))
            {
                w.Write(Encoding.ASCII.GetBytes(catatanStruk + "\n"));
            }
            w.Write(Encoding.ASCII.GetBytes("Struk ini adalah bukti pembayaran yang sah\n"));

            // 6 baris feed kosong agar melewati tear bar printer TM-U220D
            w.Write(Encoding.ASCII.GetBytes("\n\n\n\n\n\n"));

            // Potong kertas (Partial Cut): GS V 66 0
            w.Write(new byte[] { 0x1D, 0x56, 0x42, 0x00 });

            return ms.ToArray();
        }

        private static string Tengah(string teks, int lebar)
        {
            if (teks.Length >= lebar) return teks.Substring(0, lebar);
            int spasiKiri = (lebar - teks.Length) / 2;
            return teks.PadLeft(teks.Length + spasiKiri).PadRight(lebar);
        }

        private static string BarisDuaKolom(string kiri, string kanan)
        {
            int spasi = MaxCols - kiri.Length - kanan.Length;
            if (spasi < 1)
            {
                int maxKiri = Math.Max(0, MaxCols - kanan.Length - 1);
                kiri = kiri.Length > maxKiri ? kiri.Substring(0, maxKiri) : kiri;
                spasi = Math.Max(1, MaxCols - kiri.Length - kanan.Length);
            }
            return kiri + new string(' ', spasi) + kanan;
        }
    }
}
