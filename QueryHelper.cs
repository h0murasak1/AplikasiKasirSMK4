using System.Data;
using Microsoft.Data.Sqlite;

namespace AplikasiKasirSMK4
{
    /// <summary>
    /// Membantu menjalankan query SELECT yang hasilnya dipakai sebagai
    /// sumber data DataGridView.
    /// </summary>
    /// <remarks>
    /// Driver MySQL punya kelas MySqlDataAdapter yang bisa mengisi
    /// DataTable dengan satu baris kode. SQLite tidak punya padanannya,
    /// jadi pengisiannya dilakukan manual lewat SqliteDataReader.
    /// Hasilnya tetap DataTable, sehingga kode di pemanggil tidak berubah
    /// sama sekali.
    /// <para>
    /// Nama kolom hasil query ikut terbawa ke DataTable, jadi alias seperti
    /// AS 'Nama Barang' tetap muncul sebagai header kolom di grid. Sudah
    /// diuji bahwa SQLite mempertahankan nama alias berkutip tunggal
    /// persis seperti MySQL, sehingga tidak perlu ditulis ulang.
    /// </para>
    /// </remarks>
    internal static class QueryHelper
    {
        /// <summary>
        /// Menjalankan query dan mengembalikan hasilnya sebagai DataTable.
        /// </summary>
        public static DataTable IsiTabel(string sql, SqliteConnection conn)
        {
            return IsiTabel(sql, conn, null);
        }

        /// <summary>
        /// Menjalankan query dari perintah yang sudah disiapkan pemanggil,
        /// termasuk parameternya, lalu mengembalikan hasilnya sebagai
        /// DataTable.
        /// </summary>
        /// <remarks>
        /// Bentuk ini dipakai di FormLaporan, yang membangun perintahnya
        /// lebih dulu sambil menambahkan parameter secara bertahap,
        /// termasuk cabang yang berbeda tiap tab laporan. Perintah tidak
        /// dieksekusi di sini, jadi pemanggil tetap memegang kendali atas
        /// kapan query dijalankan.
        /// </remarks>
        public static DataTable IsiTabel(SqliteCommand cmd)
        {
            using SqliteDataReader reader = cmd.ExecuteReader();
            return Isi(reader);
        }

        /// <summary>
        /// Menjalankan query yang punya parameter, dan mengembalikan
        /// hasilnya sebagai DataTable.
        /// </summary>
        /// <remarks>
        /// Nilai parameter harus diberikan sebagai pasangan nama dan nilai.
        /// Nama ditulis tanpa tanda "@", misalnya ("kode", "12345"), dan
        /// tanda "@" akan ditambahkan otomatis.
        /// </remarks>
        public static DataTable IsiTabel(string sql, SqliteConnection conn,
            IEnumerable<KeyValuePair<string, object?>>? parameter)
        {
            using SqliteCommand cmd = conn.CreateCommand();
            cmd.CommandText = sql;

            if (parameter is not null)
            {
                foreach (KeyValuePair<string, object?> p in parameter)
                {
                    cmd.Parameters.AddWithValue("@" + p.Key.TrimStart('@'), p.Value);
                }
            }

            using SqliteDataReader reader = cmd.ExecuteReader();
            return Isi(reader);
        }

        /// <summary>
        /// Menyalin hasil pembaca yang sudah dieksekusi ke dalam DataTable.
        /// </summary>
        private static DataTable Isi(SqliteDataReader reader)
        {
            var tabel = new DataTable();

            // Kolom harus terdaftar lebih dulu agar DataTable tahu jenis
            // datanya, dan supaya baris kosong tetap menghasilkan tabel
            // dengan header yang benar.
            for (int i = 0; i < reader.FieldCount; i++)
            {
                string nama = reader.GetName(i);
                if (!tabel.Columns.Contains(nama))
                {
                    tabel.Columns.Add(nama, reader.GetFieldType(i));
                }
            }

            while (reader.Read())
            {
                DataRow baris = tabel.NewRow();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    string nama = reader.GetName(i);
                    baris[nama] = reader.IsDBNull(i) ? DBNull.Value : reader.GetValue(i);
                }
                tabel.Rows.Add(baris);
            }

            return tabel;
        }
    }
}
