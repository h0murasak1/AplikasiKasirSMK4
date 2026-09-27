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
        /// Urutan percabangan di sini penting, dan urutannya tetap sama
        /// meski driver sudah diganti dari MySQL ke SQLite.
        /// <para>
        /// Saat masih memakai MySql.Data, kolom TINYINT(1) sampai ke sini
        /// sebagai <see cref="bool"/>, bukan sebagai angka. Jadi kolom
        /// is_active bernilai 1 akan diterima sebagai true. Kalau bool tidak
        /// diperiksa lebih dulu, "True" gagal di-parse dan hasilnya 0, yang
        /// membuat semua user aktif terbaca sebagai nonaktif.
        /// </para>
        /// <para>
        /// Di SQLite jebakan itu sudah hilang karena kolom is_active
        /// sekarang bertipe INTEGER dan dibaca driver sebagai
        /// <see cref="long"/>, yang ditangani langsung oleh cabang di bawah.
        /// Cabang bool sengaja TIDAK dibuang: nilai boolean masih bisa
        /// muncul dari sel DataGridView yang diisi manual, dan biaya
        /// menaikkannya hampir nol.
        /// </para>
        /// <para>
        /// Kolom uang bertipe NUMERIC dibaca sebagai <see cref="double"/>,
        /// karena SQLite tidak punya desimal presisi-tetap. Konversi
        /// double ke decimal di C# memakai 15 digit signifikan, yang
        /// sudah lebih dari cukup untuk nilai uang dalam rupiah.
        /// </para>
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
        /// Mengubah nilai dari database atau sel DataGridView menjadi int
        /// dengan aman. Nilai yang tidak dikenali dianggap 0.
        /// </summary>
        /// <remarks>
        /// Dibuat saat pindah ke SQLite karena <c>SqliteDataReader</c> tidak
        /// punya <c>GetInt32(string namaKolom)</c> seperti yang dimiliki
        /// <c>MySqlDataReader</c>. Para pemanggil cukup menulis
        /// <c>AmbilInt(reader["id_user"])</c> yang lebih ringkas daripada
        /// menulis <c>reader.GetInt32(reader.GetOrdinal("id_user"))</c>.
        /// </remarks>
        public static int AmbilInt(object? nilai)
        {
            decimal angka = AmbilDecimal(nilai);
            if (angka > int.MaxValue)
            {
                return int.MaxValue;
            }
            if (angka < int.MinValue)
            {
                return int.MinValue;
            }
            return (int)angka;
        }

        /// <summary>
        /// Format tampilan jumlah (qty/stok) gaya Indonesia untuk bilangan bulat.
        /// </summary>
        /// <remarks>
        /// Qty dan stok sengaja dibulatkan karena aplikasi menjual per satuan
        /// utuh (pcs). Kolom database tetap bertipe NUMERIC, dua desimal,
        /// dijaga CHECK ROUND(x,2) = x.
        /// <para>
        /// Pembulatan di sini hanya jaring pengaman terakhir, bukan cara
        /// mem-parse. Nilai sudah divalidasi lebih dulu oleh
        /// TryParseBilanBulat, sehingga angka pecahan tidak pernah diam-diam
        /// berubah menjadi bilangan bulat.
        /// </para>
        /// </remarks>
        public static string FormatJumlah(decimal nilai)
        {
            return Math.Round(nilai, 0, MidpointRounding.AwayFromZero)
                .ToString("N0", new CultureInfo("id-ID"));
        }

        /// <summary>
        /// Mem-parse bilangan bulat untuk kolom stok dan qty.
        /// </summary>
        /// <remarks>
        /// Sengaja ketat: pemisah koma atau titik **ditolak**, bukan diabaikan.
        /// Sebelumnya input "1,5" dibersihkan menjadi "15" sehingga nilainya
        /// 10 kali lipat lebih besar tanpa ada pesan apa pun. Itu terutama
        /// berbahaya karena kolom qty dan stok menentukan jumlah yang ditagih
        /// dan jumlah barang yang dianggap tersedia.
        /// </remarks>
        /// <example>
        /// Menerima: "5" "10" "1.000" "1,000" "2.500.000"
        /// Menolak: "1,5" "1.5" "0,25" "abc" "" "-3"
        /// </example>
        public static bool TryParseBilanBulat(string? teks, out int nilai, out string pesan)
        {
            nilai = 0;
            pesan = string.Empty;

            string s = (teks ?? string.Empty).Trim().Replace(" ", string.Empty);

            if (s.Length == 0)
            {
                pesan = "nilai masih kosong";
                return false;
            }

            // Digit dan pemisah ribuan boleh lewat. Sisanya ditolak.
            if (!s.All(c => char.IsDigit(c) || c == '.' || c == ','))
            {
                pesan = "hanya boleh diisi angka bulat, tanpa huruf atau tanda lain";
                return false;
            }

            // Cuma satu jenis pemisah boleh dipakai, supaya "1.000,00" yang
            // ambigu tidak bisa lolos.
            int jumlahTitik = s.Count(c => c == '.');
            int jumlahKoma = s.Count(c => c == ',');
            if (jumlahTitik > 0 && jumlahKoma > 0)
            {
                pesan = "cuma boleh satu jenis pemisah ribuan, titik atau koma";
                return false;
            }

            // Pemisah ribuan hanya sah kalau memang mengelompokkan tiga digit,
            // contoh "1.000" atau "2.500.000". Tanpa syarat ini "1.5" ikut
            // diterima dan berubah artinya.
            char pemisah = jumlahTitik > 0 ? '.' : (jumlahKoma > 0 ? ',' : '\0');
            if (pemisah != '\0')
            {
                string[] kelompok = s.Split(pemisah);

                // Kelompok pertama boleh 1 sampai 3 digit ("1.000" sah),
                // kelompok berikutnya wajib tepat 3 digit ("1.2345" tidak sah).
                bool sah = kelompok.Length >= 2
                    && kelompok[0].Length is >= 1 and <= 3
                    && kelompok.Skip(1).All(k => k.Length == 3);

                if (!sah)
                {
                    pesan = "pemisah ribuan harus berkelompok tiga digit (contoh: 1.000)";
                    return false;
                }
            }

            string hanyaAngka = new string(s.Where(char.IsDigit).ToArray());

            // Batas 9 digit untuk stok sudah jauh melebihi kebutuhan toko.
            // Kolomnya bertipe NUMERIC di SQLite, jadi batas ini dipegang
            // oleh aplikasi, bukan oleh database.
            if (hanyaAngka.Length > 9)
            {
                pesan = "angka terlalu besar";
                return false;
            }

            if (!int.TryParse(hanyaAngka, out nilai))
            {
                nilai = 0;
                pesan = "angka terlalu besar";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Menyatakan apakah satu ketikan boleh masuk ke kolom angka bulat.
        /// </summary>
        /// <remarks>
        /// Dipasang di event KeyPress kolom stok dan qty supaya input tidak
        /// mungkin menjadi tidak valid sejak awal. Hanya digit dan tombol
        /// kontrol (Backspace, Enter, Delete) yang diizinkan, sehingga
        /// koma dan titik tidak pernah sampai ke parser dan tidak pernah
        /// dibersihkan diam-diam menjadi angka yang berbeda.
        /// <para>
        /// Fungsi ini murni dan bisa diuji tanpa membuat form, karena
        /// <see cref="KeyPressEventArgs"/> tidak punya constructor publik.
        /// </para>
        /// </remarks>
        public static bool BolehMasukAngka(char kunci)
        {
            // char.IsControl = Backspace, Enter, Delete, dan sejenisnya.
            return char.IsControl(kunci) || char.IsDigit(kunci);
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
