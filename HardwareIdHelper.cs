using System.Management;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32;

namespace AplikasiKasirSMK4
{
    /// <summary>
    /// Menghasilkan Hardware ID unik berbasis komponen fisik komputer (Motherboard, CPU, BIOS, Disk, Registry).
    /// Digunakan untuk mengunci lisensi aplikasi hanya pada satu PC tertentu.
    /// </summary>
    public static class HardwareIdHelper
    {
        private static string? _cachedHardwareId;

        public static string AmbilHardwareId()
        {
            if (!string.IsNullOrWhiteSpace(_cachedHardwareId))
            {
                return _cachedHardwareId;
            }

            var sb = new StringBuilder();

            // 1. Motherboard Serial Number
            string mbSerial = AmbilWmi("Win32_BaseBoard", "SerialNumber");
            sb.Append("MB:").Append(mbSerial).Append(';');

            // 2. Processor ID
            string cpuId = AmbilWmi("Win32_Processor", "ProcessorId");
            sb.Append("CPU:").Append(cpuId).Append(';');

            // 3. BIOS Serial Number
            string biosSerial = AmbilWmi("Win32_BIOS", "SerialNumber");
            sb.Append("BIOS:").Append(biosSerial).Append(';');

            // 4. Windows MachineGuid (Registry unik per instalasi OS)
            string machineGuid = AmbilMachineGuidRegistry();
            sb.Append("GUID:").Append(machineGuid).Append(';');

            // 5. Serial Nomor Harddisk Utama
            string diskSerial = AmbilWmi("Win32_DiskDrive", "SerialNumber");
            sb.Append("DISK:").Append(diskSerial);

            string gabungan = sb.ToString();

            // Hitung SHA-256 hash
            byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(gabungan));

            // Format ke dalam 16 karakter Hex yang mudah dibaca: SMK4-XXXX-XXXX-XXXX-XXXX
            string hex = Convert.ToHexString(hashBytes).ToUpperInvariant();
            _cachedHardwareId = $"SMK4-{hex.Substring(0, 4)}-{hex.Substring(4, 4)}-{hex.Substring(8, 4)}-{hex.Substring(12, 4)}";

            return _cachedHardwareId;
        }

        private static string AmbilWmi(string wmiClass, string propertyName)
        {
            try
            {
                using var searcher = new ManagementObjectSearcher($"SELECT {propertyName} FROM {wmiClass}");
                foreach (ManagementObject obj in searcher.Get())
                {
                    object? val = obj[propertyName];
                    if (val != null)
                    {
                        string str = val.ToString()?.Trim() ?? "";
                        if (!string.IsNullOrWhiteSpace(str) &&
                            !str.Equals("None", StringComparison.OrdinalIgnoreCase) &&
                            !str.Equals("To be filled by O.E.M.", StringComparison.OrdinalIgnoreCase) &&
                            !str.Equals("Default string", StringComparison.OrdinalIgnoreCase))
                        {
                            return str;
                        }
                    }
                }
            }
            catch
            {
                // Fallback jika WMI dibatasi
            }

            return "DEFAULT";
        }

        private static string AmbilMachineGuidRegistry()
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography");
                if (key != null)
                {
                    object? val = key.GetValue("MachineGuid");
                    if (val != null)
                    {
                        return val.ToString()?.Trim() ?? "";
                    }
                }
            }
            catch
            {
                // Fallback jika akses registry dibatasi
            }

            return Environment.MachineName;
        }
    }
}
