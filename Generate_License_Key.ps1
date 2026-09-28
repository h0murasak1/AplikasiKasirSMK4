<#
.SYNOPSIS
    Generator Kunci Aktivasi Lisensi Sistem POS SMK Negeri 4 (KHUSUS DEVELOPER FARHAN)
.DESCRIPTION
    Skrip ini digunakan oleh Developer untuk membuat Kunci Aktivasi resmi berdasarkan Kode Mesin (Hardware ID) PC sekolah.
#>

param(
    [string]$HardwareId,
    [string]$MasaAktif = "PERMANEN" # PERMANEN atau jumlah hari (misal 365)
)

$SecretKey = "Farhan_SMK4_POS_MasterSecret_Key_2026_@k$!r"

function Hitung-HmacSha256($data, $secret) {
    $hmac = New-Object System.Security.Cryptography.HMACSHA256
    $hmac.Key = [System.Text.Encoding]::UTF8.GetBytes($secret)
    $hash = $hmac.ComputeHash([System.Text.Encoding]::UTF8.GetBytes($data))
    return ([System.BitConverter]::ToString($hash) -replace '-').ToUpperInvariant()
}

Clear-Host
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "   KEY GENERATOR RESMI - SISTEM POS SMK NEGERI 4            " -ForegroundColor Yellow
Write-Host "   Khusus Penggunaan Developer (Farhan)                     " -ForegroundColor Gray
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host ""

if ([string]::IsNullOrWhiteSpace($HardwareId)) {
    $HardwareId = Read-Host ">> Masukkan Kode Mesin PC Sekolah (contoh: SMK4-XXXX-XXXX-XXXX-XXXX)"
}

$HardwareId = $HardwareId.Trim().ToUpperInvariant()
if ($HardwareId -notmatch "^SMK4-[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{4}$") {
    Write-Host "PERINGATAN: Format Hardware ID tidak biasa ($HardwareId), tetap diproses..." -ForegroundColor DarkYellow
}

Write-Host ""
Write-Host "Pilih Tipe Masa Aktif Lisensi:" -ForegroundColor Cyan
Write-Host "1. Permanen / Seumur Hidup (Rekomendasi)"
Write-Host "2. 1 Tahun (365 Hari)"
Write-Host "3. Kustom (Tentukan jumlah hari)"
$pilihan = Read-Host "Pilihan [1/2/3, default 1]"

$activationKey = ""

if ($pilihan -eq "2") {
    $expDate = (Get-Date).AddYears(1)
    $expStr = $expDate.ToString("yyyyMMdd")
    $payload = "$HardwareId|EXP:$expStr"
    $hash = Hitung-HmacSha256 $payload $SecretKey
    $activationKey = "ACT4-E-$expStr-$($hash.Substring(0,4))-$($hash.Substring(4,4))-$($hash.Substring(8,4))"
    $tipeTeks = "Berlaku 1 Tahun (s/d $($expDate.ToString('dd MMMM yyyy')))"
} elseif ($pilihan -eq "3") {
    $hari = Read-Host "Masukkan jumlah hari aktif (misal 30)"
    $expDate = (Get-Date).AddDays([int]$hari)
    $expStr = $expDate.ToString("yyyyMMdd")
    $payload = "$HardwareId|EXP:$expStr"
    $hash = Hitung-HmacSha256 $payload $SecretKey
    $activationKey = "ACT4-E-$expStr-$($hash.Substring(0,4))-$($hash.Substring(4,4))-$($hash.Substring(8,4))"
    $tipeTeks = "Berlaku $hari Hari (s/d $($expDate.ToString('dd MMMM yyyy')))"
} else {
    $payload = "$HardwareId|PERPETUAL"
    $hash = Hitung-HmacSha256 $payload $SecretKey
    $activationKey = "ACT4-P-$($hash.Substring(0,4))-$($hash.Substring(4,4))-$($hash.Substring(8,4))-$($hash.Substring(12,4))"
    $tipeTeks = "Permanen / Seumur Hidup"
}

Write-Host ""
Write-Host "==================== HASIL KUNCI AKTIVASI ====================" -ForegroundColor Green
Write-Host "Kode Mesin : $HardwareId" -ForegroundColor White
Write-Host "Tipe       : $tipeTeks" -ForegroundColor White
Write-Host "Kunci      : $activationKey" -ForegroundColor Yellow
Write-Host "==============================================================" -ForegroundColor Green
Write-Host ""

try {
    Set-Clipboard -Value $activationKey
    Write-Host "[OK] Kunci Aktivasi berhasil disalin ke clipboard! Siap dikirim ke sekolah via WA." -ForegroundColor Green
} catch {
    # Abaikan jika clipboard gagal diakses
}

Write-Host ""
Write-Host "Tekan tombol ENTER untuk keluar..."
try {
    $null = Read-Host
} catch {
    # Abaikan jika di lingkungan non-interaktif
}
