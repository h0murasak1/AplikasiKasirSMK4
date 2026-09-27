namespace AplikasiKasirSMK4
{
    /// <summary>
    /// Perintah SQL untuk satu baris profil toko.
    /// </summary>
    /// <remarks>
    /// Dipisah dari formnya dengan alasan yang sama seperti SqlMaster:
    /// perintah SQL bisa diperiksa pengujian otomatis, sedangkan
    /// tampilan tidak bisa. Yang diuji di sini adalah hal yang paling
    /// berisiko, yaitu bahwa tabel profil toko tidak pernah bisa
    /// tumbuh menjadi lebih dari satu baris, dan bahwa PPN hidup
    /// tidak mungkin tersimpan tanpa persennya.
    /// </remarks>
    internal static class SqlProfilToko
    {
        /// <summary>
        /// Kolom yang dibaca dan ditulis form profil.
        /// </summary>
        /// <remarks>
        /// Urutannya harus sama persis antara perintah baca dan
        /// perintah tulis, karena form memetakan nilainya berdasarkan
        /// nama kolom, bukan urutan. Daftar ini tetap dipakai supaya
        /// kedua perintah tidak bisa berbeda diam-diam.
        /// </remarks>
        internal const string DaftarKolom =
            "nama_toko, nama_pemilik, alamat, telepon, "
          + "email, npwp, catatan_struk, ppn_aktif, ppn_persen";

        /// <summary>
        /// Perintah membaca baris profil toko yang ada.
        /// </summary>
        /// <remarks>
        /// Syaratnya ditulis dengan id_toko = 1, bukan tanpa syarat.
        /// Kalau someday barisnya dihapus, perintah tanpa syarat akan
        /// membaca baris pertama yang ditemukan, dan form bisa saja
        /// menampilkan profil toko lain yang bukan miliknya.
        /// </remarks>
        internal static string Baca()
        {
            return "SELECT " + DaftarKolom
                 + " FROM tb_profil_toko WHERE id_toko = 1";
        }

        /// <summary>
        /// Perintah menyimpan ke baris profil yang sudah ada.
        /// </summary>
        internal static string Perbarui()
        {
            return "UPDATE tb_profil_toko SET "
                 + "nama_toko = @nama, nama_pemilik = @pemilik, "
                 + "alamat = @alamat, telepon = @telepon, email = @email, "
                 + "npwp = @npwp, catatan_struk = @catatan, "
                 + "ppn_aktif = @ppnAktif, ppn_persen = @ppnPersen, "
                 + "diperbarui_pada = strftime('%Y-%m-%d %H:%M:%S', 'now', 'localtime') "
                 + "WHERE id_toko = 1";
        }

        /// <summary>
        /// Perintah membuat baris profil kalau sebelumnya belum ada.
        /// </summary>
        /// <remarks>
        /// id_toko ditulis sebagai angka 1 di teks perintah, bukan
        /// lewat parameter. Nilainya bukan pilihan pengguna: tabel ini
        /// memang dibatasi satu baris oleh CHECK, dan angka 1 adalah
        /// satu-satunya nilai yang dibolehkan di sana.
        /// </remarks>
        internal static string Sisip()
        {
            return "INSERT INTO tb_profil_toko "
                 + "(id_toko, " + DaftarKolom + ") "
                 + "VALUES (1, @nama, @pemilik, @alamat, @telepon, "
                 + "@email, @npwp, @catatan, @ppnAktif, @ppnPersen)";
        }

        /// <summary>
        /// Memasang parameter yang isinya sama di kedua perintah tulis.
        /// </summary>
        /// <remarks>
        /// Kotak yang dibiarkan kosong dikirim sebagai DBNull, bukan
        /// string kosong. Bedanya nyata di laporan: isnull()
        /// membedakan keduanya, sedangkan string kosong akan ikut
        /// terhitung sebagai teks yang terisi.
        /// </remarks>
        internal static void PasangTeks(
            Microsoft.Data.Sqlite.SqliteCommand cmd,
            string pemilik, string alamat, string telepon,
            string email, string npwp, string catatan)
        {
            cmd.Parameters.AddWithValue("@pemilik", TeksAtauKosong(pemilik));
            cmd.Parameters.AddWithValue("@alamat", TeksAtauKosong(alamat));
            cmd.Parameters.AddWithValue("@telepon", TeksAtauKosong(telepon));
            cmd.Parameters.AddWithValue("@email", TeksAtauKosong(email));
            cmd.Parameters.AddWithValue("@npwp", TeksAtauKosong(npwp));
            cmd.Parameters.AddWithValue("@catatan", TeksAtauKosong(catatan));
        }

        private static object TeksAtauKosong(string teks)
        {
            string bersih = teks.Trim();
            return bersih.Length == 0 ? DBNull.Value : (object)bersih;
        }
    }
}
