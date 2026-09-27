namespace AplikasiKasirSMK4
{
    /// <summary>
    /// Satu berkas migrasi skema yang sudah dibaca dari folder "migrasi".
    /// </summary>
    /// <remarks>
    /// Nomor diambil dari digit-digit di depan nama berkas, misalnya
    /// "002_tahap2.sql" berarti versi 2. Urutan eksekusi selalu
    /// menurut nomor, bukan menurut nama berkas apa adanya.
    /// <para>
    /// Berkas migrasi tidak pernah diubah setelah rilis. Kalau ada
    /// kesalahan di dalamnya, perbaikannya dibuat sebagai berkas
    /// baru dengan nomor berikutnya, karena berkas yang sudah pernah
    /// dijalankan di komputer lain tidak bisa diubah diam-diam.
    /// </para>
    /// </remarks>
    internal sealed class BerkasMigrasi
    {
        /// <summary>Nomor versi, diambil dari depan nama berkas.</summary>
        public int Nomor { get; init; }

        /// <summary>Nama berkas lengkap, untuk pesan galat.</summary>
        public string Nama { get; init; } = string.Empty;

        /// <summary>Path penuh ke berkas SQL.</summary>
        public string Path { get; init; } = string.Empty;

        public override string ToString() => Nomor + " (" + Nama + ")";
    }
}
