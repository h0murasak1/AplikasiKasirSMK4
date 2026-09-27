namespace AplikasiKasirSMK4
{

    /// <summary>
    /// Macam isi kolom pada form master generik.
    /// </summary>
    /// <remarks>
    /// Macam ini menentukan dua hal sekaligus: control apa yang dibuat,
    /// dan bagaimana nilainya dibaca serta diperiksa sebelum disimpan.
    /// Menambah kolom baru cukup memilih salah satu dari tiga nilai ini,
    /// tidak perlu menulis ulang validasi.
    /// </remarks>
    internal enum MacamKolomMaster
    {
        /// <summary>Teks satu baris, misalnya nama dan telepon.</summary>
        Teks,

        /// <summary>Teks panjang beberapa baris, misalnya keterangan.</summary>
        TeksPanjang,

        /// <summary>
        /// Angka pecahan untuk uang dan persentase.
        /// </summary>
        /// <remarks>
        /// Ketikannya sengaja tidak disaring digit, sama seperti kolom
        /// harga di FormBarang. Alasannya, pemformatan ribuan otomatis
        /// saat mengetik pernah merusak nilai: "5000,50" pernah berubah
        /// jadi "50.050", jadi seratus kali lipat. Angka bulat yang
        /// benar-benar bulat, misalnya stok, tetap dikunci lewat
        /// saringan ketik, tapi kolom komisi tidak masuk kategori itu.
        /// Nilainya baru diperiksa saat disimpan.
        /// </remarks>
        Desimal
    }

    /// <summary>
    /// Deskripsi satu kolom isian pada form master generik.
    /// </summary>
    internal sealed class KolomMaster
    {
        /// <summary>Label yang tampil di sebelah kiri kotak isian.</summary>
        public string Label { get; init; } = string.Empty;

        /// <summary>Nama kolom di database. Tidak boleh kosong.</summary>
        public string Kolom { get; init; } = string.Empty;

        /// <summary>Macam isian yang dibuat untuk kolom ini.</summary>
        public MacamKolomMaster Macam { get; init; } = MacamKolomMaster.Teks;

        /// <summary>Apakah kolom ini wajib diisi sebelum bisa disimpan.</summary>
        public bool Wajib { get; init; }

        /// <summary>
        /// Batas bawah untuk kolom angka. Null berarti tanpa batas bawah.
        /// </summary>
        public decimal? Minimum { get; init; }

        /// <summary>
        /// Batas atas untuk kolom angka. Null berarti tanpa batas atas.
        /// </summary>
        public decimal? Maksimum { get; init; }

        /// <summary>
        /// Berapa karakter maksimal untuk kolom teks. Null berarti
        /// tidak dibatasi oleh aplikasi, hanya oleh database.
        /// </summary>
        public int? PanjangMaks { get; init; }

        /// <summary>
        /// Keterangan tambahan yang muncul di sebelah kanan kotak isian.
        /// Null berarti tidak ditampilkan.
        /// </summary>
        public string? Petunjuk { get; init; }

        /// <summary>
        /// Berapa nilai yang dikirim kalau kotak isian dibiarkan kosong.
        /// </summary>
        /// <remarks>
        /// Dua alasan yang berbeda perlu dibedakan di sini.
        /// <para>
        /// Yang pertama, kolom yang database mewajibkan NOT NULL dengan
        /// nilai bawaan nol, misalnya komisi_persen. Kalau kotak dikosongkan
        /// berarti "sudah tidak mendapat komisi", dan angka nol yang tepat.
        /// </para>
        /// <para>
        /// Yang kedua, kolom opsional seperti keterangan, yang kosong
        /// berarti "tidak diisi" dan harus dikirim sebagai NULL. Kalau
        /// kolom seperti ini ikut diisi nol, database akan menyimpan
        /// angka di tempat yang seharusnya kosong, dan laporan bisa saja
        /// menjumlahkannya tanpa sengaja.
        /// <para>
        /// Properti ini null kalau kotak kosong tidak boleh berarti
        /// nilai apa pun, sehingga pemanggil bisa menolaknya.
        /// </para>

        /// </remarks>
        public decimal? KosongJadiNol { get; init; }

        /// <summary>
        /// Menentukan nilai yang akan dikirim ke database.
        /// </summary>
        /// <remarks>
        /// Untuk kolom desimal, teks yang tidak bisa dibaca dianggap
        /// tidak valid dan pemeriksaan batas dilakukan pemanggil. Untuk
        /// kolom opsional, kotak yang kosong berarti null, bukan nol,
        /// supaya "belum diisi" berbeda dari "sengaja diisi nol".
        /// </remarks>
        public object? NilaiDari(string teks)
        {
            string bersih = teks.Trim();

            if (bersih.Length == 0)
            {
                return KosongJadiNol is decimal isiNol ? isiNol : (object?)null;
            }

            switch (Macam)
            {
                case MacamKolomMaster.Desimal:
                    // Dicoba lewat InputHelper, bukan decimal.TryParse
                    // biasa, supaya "1.250" dibaca sebagai seribu dua ratus
                    // lima puluh dan bukan satu koma dua lima. Aturan
                    // pemisah yang sama sudah dipakai di FormBarang dan
                    // FormKasir, jadi kasir cukup hafal satu cara menulis.
                    return InputHelper.TryParseNominal(bersih, out decimal d, out _)
                        ? d
                        : (object?)null;

                default:
                    return bersih;
            }
        }
    }

    /// <summary>
    /// Gambaran satu tabel master: nama tabel, kolom mana yang jadi
    /// identitas, dan kolom apa saja yang mengisi form.
    /// </summary>
    /// <remarks>
    /// Empat tabel master punya bentuk yang sengaja dibuat sama, dan
    /// metadata inilah yang menjelaskan bentuk itu ke aplikasi. Dengan
    /// satu FormMaster, keempat tabel itu bisa dilayani tanpa menulis
    /// form terpisah.
    /// <para>
    /// Yang tidak boleh terjadi: nama tabel atau kolom diambil dari
    /// input user. Semua nilai di sini berasal dari kode, bukan dari
    /// database, jadi aman disambung langsung ke SQL. Nilai yang dari
    /// user selalu lewat parameter, seperti di form lain.
    /// </para>
    /// </remarks>
    internal sealed class MetadataMaster
    {
        /// <summary>Nama tabel di database, misalnya tb_jenis_barang.</summary>
        public string Tabel { get; init; } = string.Empty;

        /// <summary>Judul yang tampil di bagian atas form.</summary>
        public string Judul { get; init; } = string.Empty;

        /// <summary>Nama menu, tanpa huruf besar di awal.</summary>
        public string NamaMenu { get; init; } = string.Empty;

        /// <summary>Kolom yang jadi identitas baris, misalnya id_jenis.</summary>
        public string KolomId { get; init; } = string.Empty;

        /// <summary>Kolom nama utama, misalnya nama_jenis.</summary>
        public string KolomNama { get; init; } = string.Empty;

        /// <summary>Label untuk kolom nama utama di form.</summary>
        public string LabelNama { get; init; } = string.Empty;

        /// <summary>
        /// Kolom penanda aktif atau tidak. Semua tabel master punya
        /// is_active, jadi tidak perlu dibuat per metadata.
        /// </summary>
        public string KolomAktif => "is_active";

        /// <summary>
        /// Kolom isian selain kolom nama. Daftar ini yang menentukan
        /// isi form, urutannya juga menentukan urutan tampil.
        /// </summary>
        public IReadOnlyList<KolomMaster> KolomTambahan { get; init; }
            = Array.Empty<KolomMaster>();

        /// <summary>
        /// Kolom yang ditampilkan sebagai kolom pertama di daftar,
        /// di sebelah kiri kolom nama.
        /// </summary>
        public string? KolomKode { get; init; }

        /// <summary>Label untuk kolom kode di form.</summary>
        public string LabelKode { get; init; } = string.Empty;

        /// <summary>
        /// Daftar semua kolom yang boleh ditulis, yaitu kolom kode bila
        /// ada, lalu kolom nama, lalu seluruh kolom tambahan.
        /// </summary>
        /// <remarks>
        /// Kolom kode ikut masuk karena Perbarui menyusun klausanya dari
        /// daftar ini. Kalau kode tidak ikut, nilai kode_sales bisa
        /// terkirim sebagai parameter tapi tidak pernah masuk ke
        /// klausanya SET, jadi mengedit kode sales seperti tidak
        /// berpengaruh sama sekali.
        /// </remarks>
        public IEnumerable<string> SemuaKolomIsian()
        {
            if (KolomKode is not null)
            {
                yield return KolomKode;
            }

            yield return KolomNama;
            foreach (KolomMaster k in KolomTambahan)
            {
                yield return k.Kolom;
            }
        }

        /// <summary>
        /// Daftar seluruh kolom isian dalam bentuk yang siap disambung
        /// ke klausa SET, misal "nama_jenis = @nama_jenis,
        /// keterangan = @keterangan".
        /// </summary>
        public string SetKolomIsian(string awalanParameter = "@")
        {
            var bagian = new List<string>();
            foreach (string kolom in SemuaKolomIsian())
            {
                bagian.Add(kolom + " = " + awalanParameter + kolom);
            }

            return string.Join(", ", bagian);
        }
    }

    /// <summary>
    /// Kumpulan metadata untuk seluruh tabel master generik.
    /// </summary>
    /// <remarks>
    /// Semua definisi ada di satu tempat supaya mudah ditambah dan
    /// mudah dibandingkan. Tambah master baru cukup menambah satu
    /// MetadataMaster di Daftar, tidak perlu form baru.
    /// </remarks>
    internal static class MasterData
    {
        /// <summary>Jenis barang, yaitu kategori barang.</summary>
        public static readonly MetadataMaster Jenis = new()
        {
            Tabel = "tb_jenis_barang",
            Judul = "Jenis Barang",
            NamaMenu = "Jenis Barang",
            KolomId = "id_jenis",
            KolomNama = "nama_jenis",
            LabelNama = "Nama Jenis",
            KolomTambahan = new[]
            {
                new KolomMaster
                {
                    Label = "Keterangan",
                    Kolom = "keterangan",
                    Macam = MacamKolomMaster.TeksPanjang,
                    PanjangMaks = 200
                }
            }
        };

        /// <summary>Merek barang, yaitu nama dagang yang tercetak di kemasan.</summary>
        public static readonly MetadataMaster Merek = new()
        {
            Tabel = "tb_merek",
            Judul = "Merek Barang",
            NamaMenu = "Merek",
            KolomId = "id_merek",
            KolomNama = "nama_merek",
            LabelNama = "Nama Merek",
            KolomTambahan = new[]
            {
                new KolomMaster
                {
                    Label = "Keterangan",
                    Kolom = "keterangan",
                    Macam = MacamKolomMaster.TeksPanjang,
                    PanjangMaks = 200
                }
            }
        };

        /// <summary>Supplier atau pemasok barang.</summary>
        public static readonly MetadataMaster Supplier = new()
        {
            Tabel = "tb_supplier",
            Judul = "Supplier",
            NamaMenu = "Supplier",
            KolomId = "id_supplier",
            KolomNama = "nama_supplier",
            LabelNama = "Nama Supplier",
            KolomTambahan = new[]
            {
                new KolomMaster
                {
                    Label = "Nama Kontak",
                    Kolom = "nama_kontak",
                    Macam = MacamKolomMaster.Teks,
                    PanjangMaks = 100
                },
                new KolomMaster
                {
                    Label = "Telepon",
                    Kolom = "telepon",
                    Macam = MacamKolomMaster.Teks,
                    PanjangMaks = 30
                },
                new KolomMaster
                {
                    Label = "Alamat",
                    Kolom = "alamat",
                    Macam = MacamKolomMaster.TeksPanjang,
                    PanjangMaks = 300
                },
                new KolomMaster
                {
                    Label = "Keterangan",
                    Kolom = "keterangan",
                    Macam = MacamKolomMaster.TeksPanjang,
                    PanjangMaks = 200
                }
            }
        };

        /// <summary>
        /// Sales atau koeplen, orang yang menjajakan barang di luar toko.
        /// </summary>
        /// <remarks>
        /// Kolom komisi_persen di sini sudah punya batas 0 sampai 100
        /// di database. Batas yang sama diulang di metadata supaya
        /// kesalahan ketik tertangkap sebelum sampai ke database, dan
        /// supaya pesannya menyebut satuan yang dimengerti kasir.
        /// </remarks>
        public static readonly MetadataMaster Sales = new()
        {
            Tabel = "tb_sales",
            Judul = "Data Sales",
            NamaMenu = "Sales",
            KolomId = "id_sales",
            KolomNama = "nama_sales",
            LabelNama = "Nama Sales",
            KolomKode = "kode_sales",
            LabelKode = "Kode Sales",
            KolomTambahan = new[]
            {
                new KolomMaster
                {
                    Label = "Telepon",
                    Kolom = "telepon",
                    Macam = MacamKolomMaster.Teks,
                    PanjangMaks = 30
                },
                new KolomMaster
                {
                    Label = "Komisi (%)",
                    Kolom = "komisi_persen",
                    Macam = MacamKolomMaster.Desimal,
                    Minimum = 0,
                    Maksimum = 100,
                    KosongJadiNol = 0m,
                    Petunjuk = "Kosong berarti tidak menerima komisi."
                },
                new KolomMaster
                {
                    Label = "Keterangan",
                    Kolom = "keterangan",
                    Macam = MacamKolomMaster.TeksPanjang,
                    PanjangMaks = 200
                }
            }
        };

        /// <summary>
        /// Seluruh master yang dilayani FormMaster, dalam urutan tampil
        /// di menu.
        /// </summary>
        public static IReadOnlyList<MetadataMaster> Semua { get; } = new[]
        {
            Jenis,
            Merek,
            Supplier,
            Sales
        };

        /// <summary>
        /// Mencari metadata berdasarkan nama tabel.
        /// </summary>
        public static MetadataMaster? Cari(string tabel)
        {
            foreach (MetadataMaster m in Semua)
            {
                if (string.Equals(m.Tabel, tabel, StringComparison.OrdinalIgnoreCase))
                {
                    return m;
                }
            }

            return null;
        }
    }
}
