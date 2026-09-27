using Microsoft.Data.Sqlite;

namespace AplikasiKasirSMK4
{
    /// <summary>
    /// Membangun seluruh perintah SQL untuk master generik.
    /// </summary>
    /// <remarks>
    /// Class ini sengaja dipisah dari FormMaster. Alasannya, perintah
    /// SQL adalah bagian yang paling mudah salah dan paling sulit
    /// diperiksa mata, sementara form buatannya sudah jelas benar
    /// secara visual. Kalau keduanya bercampur, satu-satunya cara
    /// menguji SQL-nya adalah membuka form sungguhan, yang tidak bisa
    /// dilakukan dari pengujian otomatis.
    /// <para>
    /// Yang dijanjikan di sini sederhana: nama tabel dan nama kolom
    /// disambung langsung ke teks perintah karena keduanya berasal
    /// dari metadata di dalam program, bukan dari ketikan pengguna.
    /// Setiap nilai yang bisa diketik orang tetap lewat parameter.
    /// </para>
    /// </remarks>
    internal static class SqlMaster
    {
        /// <summary>
        /// Perintah untuk mengisi daftar di layar.
        /// </summary>
        /// <remarks>
        /// Kolom keterangan yang panjang tidak ikut ditampilkan.
        /// Kotak daftar hanya boleh memuat kolom yang bisa dibaca
        /// sekilas; paragraf tiga baris di sana akan membuat satu
        /// baris jadi setinggi tiga baris.
        /// </remarks>
        internal static string Daftar(MetadataMaster meta, bool tampilkanNonaktif, string? cari)
        {
            var sql = new System.Text.StringBuilder();
            sql.Append("SELECT ").Append(meta.KolomId).Append(" AS 'Id', ");

            if (meta.KolomKode is not null)
            {
                sql.Append(meta.KolomKode).Append(" AS 'Kode', ");
            }

            sql.Append(meta.KolomNama).Append(" AS '").Append(meta.LabelNama).Append('\'');

            foreach (KolomMaster kolom in meta.KolomTambahan)
            {
                if (kolom.Macam == MacamKolomMaster.TeksPanjang)
                {
                    continue;
                }

                sql.Append(", ").Append(kolom.Kolom)
                   .Append(" AS '").Append(kolom.Label).Append('\'');
            }

            sql.Append(", ").Append(meta.KolomAktif).Append(" AS 'Aktif' FROM ")
               .Append(meta.Tabel);

            var syarat = new List<string>();
            if (!tampilkanNonaktif)
            {
                syarat.Add(meta.KolomAktif + " = 1");
            }

            if (!string.IsNullOrWhiteSpace(cari))
            {
                syarat.Add("(" + string.Join(" OR ", KolomDicari(meta)
                    .Select(k => k + " LIKE @cari")) + ")");
            }

            if (syarat.Count > 0)
            {
                sql.Append(" WHERE ").Append(string.Join(" AND ", syarat));
            }

            // Diurutkan mengabaikan huruf besar-kecil, karena daftar
            // untuk orang, bukan untuk mesin. "ABC" tidak seharusnya
            // mendahului "abc" hanya karena huruf A lebih dulu di tabel
            // karakter.
            sql.Append(" ORDER BY ").Append(meta.KolomNama).Append(" COLLATE NOCASE ASC");
            return sql.ToString();
        }

        /// <summary>
        /// Daftar kolom yang boleh dicari lewat kotak pencarian.
        /// </summary>
        private static IEnumerable<string> KolomDicari(MetadataMaster meta)
        {
            if (meta.KolomKode is not null)
            {
                yield return meta.KolomKode;
            }

            yield return meta.KolomNama;

            foreach (KolomMaster kolom in meta.KolomTambahan)
            {
                yield return kolom.Kolom;
            }
        }

        /// <summary>
        /// Nilai parameter untuk kotak pencarian, atau null kalau
        /// kotak pencarian kosong dan tidak perlu parameter sama sekali.
        /// </summary>
        internal static string? NilaiCari(string? cari)
        {
            string bersih = (cari ?? string.Empty).Trim();
            return bersih.Length == 0 ? null : "%" + bersih + "%";
        }

        /// <summary>
        /// Perintah menyisipkan baris master baru.
        /// </summary>
        /// <remarks>
        /// Ada tiga kelompok kolom yang perlakuanunya berbeda, dan
        /// perbedaan itu sengaja ditulis eksplisit satu per satu.
        /// Kalau tidak, satu kesalahan kecil di sini akan diam-diam
        /// menuliskan teks yang sama ke semua kolom, dan galatnya baru
        /// muncul saat isi tabel dibaca orang. Build yang bersih tidak
        /// akan menangkap hal seperti itu.
        /// </remarks>
        internal static string Sisip(MetadataMaster meta, IEnumerable<string> kolomIsian)
        {
            var kolom = new List<string>(kolomIsian);
            kolom.Add(meta.KolomAktif);
            kolom.Add(KOLOM_DIBUAT);
            kolom.Add(KOLOM_DIPERBARUI);

            var tanda = new List<string>();
            foreach (string k in kolom)
            {
                tanda.Add(NilaiSisip(meta, k));
            }

            return "INSERT INTO " + meta.Tabel
                 + " (" + string.Join(", ", kolom) + ")"
                 + " VALUES (" + string.Join(", ", tanda) + ")";
        }

        /// <summary>
        /// Nilai yang ditulis di sebelah kanan tanda koma untuk satu
        /// kolom pada perintah sisip.
        /// </summary>
        /// <remarks>
        /// Penanda aktif ditulis sebagai angka 1 karena baris master
        /// yang baru selalu aktif. Dua kolom waktu ditulis memakai
        /// strftime bawaan database, karena waktu harus dicatat dalam
        /// waktu database itu sendiri, bukan waktu computer kasir.
        /// Kalau jam kasir salah atau zona waktu berbeda, tanggal di
        /// kartu stok akan melenceng. Sisanya adalah parameter biasa.
        /// </remarks>
        private static string NilaiSisip(MetadataMaster meta, string kolom)
        {
            if (kolom == meta.KolomAktif)
            {
                return "1";
            }

            if (kolom == KOLOM_DIBUAT || kolom == KOLOM_DIPERBARUI)
            {
                return WAKTU_SQL;
            }

            return "@" + kolom;
        }

        /// <summary>Nama kolom waktu saat baris pertama dibuat.</summary>
        private const string KOLOM_DIBUAT = "dibuat_pada";

        /// <summary>Nama kolom waktu saat baris terakhir diubah.</summary>
        private const string KOLOM_DIPERBARUI = "diperbarui_pada";

        /// <summary>
        /// Perintah memperbarui baris master yang sudah ada.
        /// </summary>
        internal static string Perbarui(MetadataMaster meta)
        {
            return "UPDATE " + meta.Tabel
                 + " SET " + meta.SetKolomIsian()
                 + ", " + meta.KolomAktif + " = @aktif"
                 + ", " + KOLOM_DIPERBARUI + " = " + WAKTU_SQL
                 + " WHERE " + meta.KolomId + " = @id";
        }

        /// <summary>
        /// Perintah mengaktifkan atau menonaktifkan satu baris.
        /// </summary>
        /// <remarks>
        /// Tidak ada perintah hapus. Baris master yang pernah dipakai
        /// transaksi harus tetap bisa dibaca, jadi yang diubah hanya
        /// penandanya.
        /// </remarks>
        internal static string UbahAktif(MetadataMaster meta)
        {
            return "UPDATE " + meta.Tabel
                 + " SET " + meta.KolomAktif + " = @aktif"
                 + ", " + KOLOM_DIPERBARUI + " = " + WAKTU_SQL
                 + " WHERE " + meta.KolomId + " = @id";
        }

        /// <summary>
        /// Perintah membaca satu baris master lengkap.
        /// </summary>
        /// <remarks>
        /// Yang diambil adalah seluruh kolom, bukan hanya kolom isian,
        /// karena isian form juga perlu nilai penanda aktif supaya
        /// baris nonaktif yang dibuka tidak terlihat seperti baris baru.
        /// </remarks>
        internal static string SatuBaris(MetadataMaster meta)
        {
            return "SELECT * FROM " + meta.Tabel
                 + " WHERE " + meta.KolomId + " = @id";
        }

        /// <summary>
        /// Perintah mencari id baris dengan nama yang sama.
        /// </summary>
        /// <remarks>
        /// Pencarian sengaja tidak memfilter is_active. Nama yang sama
        /// tetap bentrok dengan batasan UNIQUE walau baris lamanya
        /// sudah nonaktif, jadi kalau hanya baris aktif yang dicari,
        /// orang akan mendapat pesan galat dari database yang jauh
        /// lebih membingungkan daripada "nama ini sudah dipakai".
        /// </remarks>
        internal static string CariIdDenganNama(MetadataMaster meta)
        {
            return "SELECT " + meta.KolomId + " FROM " + meta.Tabel
                 + " WHERE " + meta.KolomNama + " = @nama COLLATE NOCASE";
        }

        /// <summary>
        /// Memasang nilai isian sebagai parameter bernama.
        /// </summary>
        /// <remarks>
        /// Nilai null dikirim sebagai DBNull, bukan sebagai string
        /// kosong. Bedanya nyata: kolom yang kosong harus tetap terbaca
        /// kosong oleh isnull() di laporan, sedangkan string kosong
        /// akan terhitung sebagai teks yang sudah diisi.
        /// </remarks>
        internal static void PasangNilai(
            SqliteCommand cmd, IEnumerable<KeyValuePair<string, object?>> nilai)
        {
            foreach (KeyValuePair<string, object?> pasangan in nilai)
            {
                cmd.Parameters.AddWithValue("@" + pasangan.Key,
                    pasangan.Value ?? DBNull.Value);
            }
        }

        /// <summary>
        /// Ekspresi waktu lokal yang dipakai di setiap perintah tulis.
        /// </summary>
        /// <remarks>
        /// Zona waktu ikut ditulis di dalamnya karena aplikasi ini
        /// dipakai di satu computer yang tidak berpindah tempat.
        /// </remarks>
        private const string WAKTU_SQL =
            "strftime('%Y-%m-%d %H:%M:%S', 'now', 'localtime')";
    }
}
