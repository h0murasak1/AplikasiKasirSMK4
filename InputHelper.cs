using System.Globalization;

namespace AplikasiKasirSMK4
{
    /// <summary>
    /// Helper untuk memvalidasi dan mem-parsing input angka nominal rupiah
    /// dari textbox, dengan dukungan pemisah ribuan.
    /// </summary>
    internal static class InputHelper
    {
        /// <summary>
        /// Mem-parse nominal rupiah. Menerima:
        ///   3000        -> 3000
        ///   3.000       -> 3000   (pemisah ribuan)
        ///   30,000      -> 30000  (pemisah ribuan)
        ///   2500.50     -> 2500,50 (desimal)
        ///   2500,50     -> 2500,50 (desimal)
        /// </summary>
        public static bool TryParseNominal(string? teks, out decimal nilai, out string pesan)
        {
            nilai = 0m;
            pesan = string.Empty;

            string s = (teks ?? string.Empty)
                .Trim()
                .Replace(" ", string.Empty)
                .Replace("\u00A0", string.Empty);   // non-breaking space

            if (s.Length == 0)
            {
                pesan = "nilai masih kosong";
                return false;
            }

            if (!s.All(c => char.IsDigit(c) || c == '.' || c == ','))
            {
                pesan = "hanya boleh diisi angka";
                return false;
            }

            int posisiPemisah = Math.Max(s.LastIndexOf('.'), s.LastIndexOf(','));
            if (posisiPemisah >= 0)
            {
                char pemisahDesimal = s[posisiPemisah];
                int panjangPecahan = s.Length - posisiPemisah - 1;
                string bagianBulat = s[..posisiPemisah];
                string bagianPecahan = s[(posisiPemisah + 1)..];

                if (panjangPecahan == 3)
                {
                    // Dua jenis pemisah sekaligus DAN tepat 3 digit di belakang
                    // berarti inputnya ambigu. Contoh "10.000,123":
                    //   bisa berarti 10.000,123  (sepuluh ribu seratus dua puluh tiga)
                    //   bisa berarti 10.000.123  (sepuluh juta)
                    // Salah tebak berarti selisih 1000 kali lipat, jadi input
                    // seperti ini ditolak agar kasir memeriksa ulang. Nominal
                    // di database hanya punya 2 desimal, jadi 3 desimal pun
                    // memang tidak pernah dipakai.
                    bool adaPemisahLain = s.Contains(pemisahDesimal == '.' ? ',' : '.');
                    if (adaPemisahLain)
                    {
                        pesan = "format ambigu, tulis maksimal 2 angka desimal "
                            + "(contoh: 10.000,12)";
                        return false;
                    }

                    // Hanya satu jenis pemisah, semua dianggap pemisah ribuan.
                    // Menangani "3.000", "30,000", "1.250.000", "1,250,000".
                    s = s.Replace(".", "").Replace(",", "");
                }
                else if (panjangPecahan is 1 or 2)
                {
                    // Pemisah terakhir adalah desimal, pemisah lain adalah ribuan.
                    // Menangani "2500.50", "2500,50", "1.250,50", "1,250.50".
                    string bulat = bagianBulat
                        .Replace(pemisahDesimal == '.' ? "," : ".", "");
                    s = bulat + "." + bagianPecahan;
                }
                else
                {
                    pesan = "format angka tidak valid";
                    return false;
                }
            }

            // AllowDecimalPoint karena hasil normalisasi memakai titik sebagai desimal.
            if (!decimal.TryParse(s, NumberStyles.AllowDecimalPoint,
                    CultureInfo.InvariantCulture, out nilai))
            {
                nilai = 0m;
                pesan = "format angka tidak valid";
                return false;
            }

            if (nilai < 0m)
            {
                nilai = 0m;
                pesan = "tidak boleh bernilai negatif";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Format tampilan nominal sesuai gaya Indonesia.
        /// Contoh: 12500 -&gt; "12.500", 2500.5 -&gt; "2.500,50".
        /// </summary>
        public static string FormatNominal(decimal nilai)
        {
            // Tampilkan desimal hanya bila memang ada pecahan, agar tampilan
            // selalu sama persis dengan nilai yang sebenarnya.
            string format = nilai == decimal.Truncate(nilai) ? "N0" : "N2";
            return ToGayaIndonesia(nilai.ToString(format, CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Mengubah nilai dari database atau sel DataGridView menjadi decimal
        /// dengan aman. Nilai yang tidak dikenali dianggap 0, bukan melempar
        /// exception, karena pemanggilnya biasanya di dalam loop.
        /// </summary>
        /// <remarks>
        /// Urutan percabangan di sini penting. Driver MySql.Data mengembalikan
        /// <c>TINYINT(1)</c> sebagai <see cref="bool"/>, bukan sebagai angka.
        /// Jadi kolom <c>is_active</c> yang bernilai 1 akan sampai ke sini
        /// sebagai <c>true</c>. Kalau <c>bool</c> tidak diperiksa lebih dulu,
        /// <c>"True"</c> gagal di-parse dan hasilnya 0 - yang membuat semua
        /// user aktif terbaca sebagai nonaktif. Verifikasi ada di kelompok
        /// pengujian "P. Tipe data dari MySql.Data".
        /// </remarks>
        public static decimal AmbilDecimal(object? nilai)
        {
            return nilai switch
            {
                null => 0m,
                decimal d => d,
                bool b => b ? 1m : 0m,
                int i => i,
                uint ui => ui,
                long l => l,
                ulong ul => ul,
                short s => s,
                ushort us => us,
                sbyte sb => sb,
                byte by => by,
                double dd => (decimal)dd,
                float f => (decimal)f,
                _ => decimal.TryParse(Convert.ToString(nilai, CultureInfo.InvariantCulture),
                        NumberStyles.Any, CultureInfo.InvariantCulture, out decimal hasil)
                    ? hasil
                    : 0m
            };
        }

        /// <summary>
        /// Format tampilan jumlah (qty/stok) sesuai gaya Indonesia.
        /// Pecahan hanya ditampilkan bila memang ada, agar 1 tetap tampil "1"
        /// dan bukan "1,00". Contoh: 1 -&gt; "1", 1.5 -&gt; "1,5", 0.25 -&gt; "0,25".
        /// </summary>
        public static string FormatJumlah(decimal nilai)
        {
            // Qty barang berupa bilangan bulat / pcs
            return Math.Round(nilai, 0, MidpointRounding.AwayFromZero)
                .ToString("N0", new CultureInfo("id-ID"));
        }

        /// <summary>
        /// Memberikan format titik ribuan otomatis pada HopeTextBox (misal 5000 -> 5.000)
        /// dengan tetap menjaga posisi kursor pengguna saat mengetik.
        /// </summary>
        public static void FormatRibuanOtomatis(ReaLTaiizor.Controls.HopeTextBox textBox, ref bool isFormatting)
        {
            if (isFormatting) return;

            string raw = textBox.Text;
            if (string.IsNullOrWhiteSpace(raw)) return;

            int oldCaret = textBox.SelectionStart;

            // Hitung berapa digit angka sebelum posisi kursor saat ini
            int digitsBeforeCursor = 0;
            for (int i = 0; i < Math.Min(oldCaret, raw.Length); i++)
            {
                if (char.IsDigit(raw[i]))
                {
                    digitsBeforeCursor++;
                }
            }

            // Ambil hanya karakter angka
            string digitsOnly = new string(raw.Where(char.IsDigit).ToArray());
            if (digitsOnly.Length == 0)
            {
                isFormatting = true;
                textBox.Text = string.Empty;
                isFormatting = false;
                return;
            }

            // Batasi panjang agar tidak overflow (maks 14 digit)
            if (digitsOnly.Length > 14)
            {
                digitsOnly = digitsOnly[..14];
            }

            if (long.TryParse(digitsOnly, out long number))
            {
                string formatted = number.ToString("N0", new CultureInfo("id-ID"));
                if (formatted == raw) return;

                isFormatting = true;
                textBox.Text = formatted;

                // Hitung posisi kursor baru yang sesuai
                int newCaret = 0;
                int countDigits = 0;
                for (int i = 0; i < formatted.Length; i++)
                {
                    if (char.IsDigit(formatted[i]))
                    {
                        countDigits++;
                    }
                    if (countDigits == digitsBeforeCursor)
                    {
                        newCaret = i + 1;
                        break;
                    }
                }

                if (digitsBeforeCursor == 0)
                {
                    newCaret = 0;
                }
                else if (countDigits < digitsBeforeCursor)
                {
                    newCaret = formatted.Length;
                }

                textBox.SelectionStart = Math.Clamp(newCaret, 0, formatted.Length);
                isFormatting = false;
            }
        }

        /// <summary>
        /// Memberikan format titik ribuan otomatis pada standard TextBoxBase.
        /// </summary>
        public static void FormatRibuanOtomatis(TextBoxBase textBox, ref bool isFormatting)
        {
            if (isFormatting) return;

            string raw = textBox.Text;
            if (string.IsNullOrWhiteSpace(raw)) return;

            int oldCaret = textBox.SelectionStart;

            int digitsBeforeCursor = 0;
            for (int i = 0; i < Math.Min(oldCaret, raw.Length); i++)
            {
                if (char.IsDigit(raw[i]))
                {
                    digitsBeforeCursor++;
                }
            }

            string digitsOnly = new string(raw.Where(char.IsDigit).ToArray());
            if (digitsOnly.Length == 0)
            {
                isFormatting = true;
                textBox.Text = string.Empty;
                isFormatting = false;
                return;
            }

            if (digitsOnly.Length > 14)
            {
                digitsOnly = digitsOnly[..14];
            }

            if (long.TryParse(digitsOnly, out long number))
            {
                string formatted = number.ToString("N0", new CultureInfo("id-ID"));
                if (formatted == raw) return;

                isFormatting = true;
                textBox.Text = formatted;

                int newCaret = 0;
                int countDigits = 0;
                for (int i = 0; i < formatted.Length; i++)
                {
                    if (char.IsDigit(formatted[i]))
                    {
                        countDigits++;
                    }
                    if (countDigits == digitsBeforeCursor)
                    {
                        newCaret = i + 1;
                        break;
                    }
                }

                if (digitsBeforeCursor == 0)
                {
                    newCaret = 0;
                }
                else if (countDigits < digitsBeforeCursor)
                {
                    newCaret = formatted.Length;
                }

                textBox.SelectionStart = Math.Clamp(newCaret, 0, formatted.Length);
                isFormatting = false;
            }
        }

        /// <summary>
        /// Mengubah pemisah ribuan/desimal InvariantCulture menjadi gaya Indonesia.
        /// InvariantCulture memakai koma untuk ribuan dan titik untuk desimal,
        /// sedangkan gaya Indonesia memakai titik untuk ribuan dan koma untuk desimal.
        /// </summary>
        private static string ToGayaIndonesia(string teks)
        {
            return teks
                .Replace(',', '\u0001')
                .Replace('.', ',')
                .Replace('\u0001', '.');
        }
    }
}
