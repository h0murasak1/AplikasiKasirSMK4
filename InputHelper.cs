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
                    // Semua pemisah dianggap pemisah ribuan.
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
            string teks = nilai.ToString(format, CultureInfo.InvariantCulture);

            // InvariantCulture: koma = pemisah ribuan, titik = desimal.
            // Gaya Indonesia: titik = pemisah ribuan, koma = desimal.
            return teks
                .Replace(',', '\u0001')
                .Replace('.', ',')
                .Replace('\u0001', '.');
        }

        /// <summary>
        /// Mengubah nilai sel DataGridView menjadi decimal dengan aman.
        /// </summary>
        public static decimal AmbilDecimal(object? nilai)
        {
            if (nilai is null)
            {
                return 0m;
            }

            if (nilai is decimal d)
            {
                return d;
            }

            if (nilai is int i)
            {
                return i;
            }

            if (nilai is long l)
            {
                return l;
            }

            if (nilai is double dd)
            {
                return (decimal)dd;
            }

            return decimal.TryParse(nilai.ToString(), NumberStyles.Any,
                CultureInfo.InvariantCulture, out decimal hasil) ? hasil : 0m;
        }

        /// <summary>
        /// Mengubah nilai sel DataGridView menjadi int dengan aman.
        /// </summary>
        public static int AmbilInt(object? nilai)
        {
            return (int)AmbilDecimal(nilai);
        }
    }
}
