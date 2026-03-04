using System.Management;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32;

namespace DeviceId;

public static class DeviceIdHelper
{
    /// <summary>
    /// Gets the Device ID shown in Windows Settings > System > About > Device specifications.
    /// This is the SQMClient MachineId used by Microsoft telemetry/services.
    /// </summary>
    public static string? GetWindowsAboutDeviceId()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return null;

        try
        {
            // This is the Device ID shown in Windows Settings > About
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\SQMClient");
            var machineId = key?.GetValue("MachineId")?.ToString();
            // Remove curly braces if present for consistency
            return machineId?.Trim('{', '}');
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Gets the Windows MachineGuid from registry (Cryptography key).
    /// Unique per Windows installation. Changes on OS reinstall.
    /// </summary>
    public static string? GetMachineGuid()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return null;

        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography");
            return key?.GetValue("MachineGuid")?.ToString();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Gets the SMBIOS UUID from WMI (hardware-based).
    /// Persists across OS reinstalls. Requires Windows + WMI.
    /// </summary>
    public static string? GetSmbiosUuid()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return null;

        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT UUID FROM Win32_ComputerSystemProduct");
            foreach (var obj in searcher.Get())
            {
                var uuid = obj["UUID"]?.ToString();
                // Skip placeholder UUIDs (all zeros or FFs)
                if (!string.IsNullOrEmpty(uuid) &&
                    uuid != "FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF" &&
                    uuid != "00000000-0000-0000-0000-000000000000")
                {
                    return uuid;
                }
            }
        }
        catch
        {
            // WMI not available
        }
        return null;
    }

    /// <summary>
    /// Gets the motherboard serial number from WMI.
    /// </summary>
    public static string? GetMotherboardSerial()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return null;

        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard");
            foreach (var obj in searcher.Get())
            {
                var serial = obj["SerialNumber"]?.ToString();
                if (!string.IsNullOrEmpty(serial) && serial != "To Be Filled By O.E.M.")
                    return serial;
            }
        }
        catch { }
        return null;
    }

    /// <summary>
    /// Gets the BIOS serial number from WMI.
    /// </summary>
    public static string? GetBiosSerial()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return null;

        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BIOS");
            foreach (var obj in searcher.Get())
            {
                var serial = obj["SerialNumber"]?.ToString();
                if (!string.IsNullOrEmpty(serial) && serial != "To Be Filled By O.E.M.")
                    return serial;
            }
        }
        catch { }
        return null;
    }

    /// <summary>
    /// Gets the processor ID from WMI.
    /// </summary>
    public static string? GetProcessorId()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return null;

        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor");
            foreach (var obj in searcher.Get())
            {
                var id = obj["ProcessorId"]?.ToString();
                if (!string.IsNullOrEmpty(id))
                    return id;
            }
        }
        catch { }
        return null;
    }

    /// <summary>
    /// Gets the first physical disk serial number.
    /// </summary>
    public static string? GetDiskSerial()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return null;

        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_DiskDrive WHERE MediaType LIKE '%fixed%'");
            foreach (var obj in searcher.Get())
            {
                var serial = obj["SerialNumber"]?.ToString()?.Trim();
                if (!string.IsNullOrEmpty(serial))
                    return serial;
            }
        }
        catch { }
        return null;
    }

    /// <summary>
    /// Generates a composite hardware ID by hashing multiple hardware identifiers.
    /// More stable than individual IDs - survives some hardware changes.
    /// </summary>
    public static string? GenerateCompositeHardwareId()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return null;

        var components = new List<string>();

        // Add available hardware IDs (order matters for consistency)
        var smbios = GetSmbiosUuid();
        if (!string.IsNullOrEmpty(smbios)) components.Add($"SMBIOS:{smbios}");

        var mobo = GetMotherboardSerial();
        if (!string.IsNullOrEmpty(mobo)) components.Add($"MOBO:{mobo}");

        var bios = GetBiosSerial();
        if (!string.IsNullOrEmpty(bios)) components.Add($"BIOS:{bios}");

        var cpu = GetProcessorId();
        if (!string.IsNullOrEmpty(cpu)) components.Add($"CPU:{cpu}");

        if (components.Count == 0)
            return null;

        // Create a hash of all components
        var combined = string.Join("|", components);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(combined));
        return Convert.ToHexString(hash);
    }

    /// <summary>
    /// Gets the Windows Product ID (not unique, but useful for identification).
    /// </summary>
    public static string? GetWindowsProductId()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return null;

        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            return key?.GetValue("ProductId")?.ToString();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Gets the computer name.
    /// </summary>
    public static string GetComputerName() => Environment.MachineName;

    /// <summary>
    /// Gets comprehensive device information.
    /// </summary>
    public static DeviceInfo GetDeviceInfo()
    {
        return new DeviceInfo
        {
            ComputerName = GetComputerName(),
            WindowsAboutDeviceId = GetWindowsAboutDeviceId(),
            MachineGuid = GetMachineGuid(),
            SmbiosUuid = GetSmbiosUuid(),
            MotherboardSerial = GetMotherboardSerial(),
            BiosSerial = GetBiosSerial(),
            ProcessorId = GetProcessorId(),
            DiskSerial = GetDiskSerial(),
            WindowsProductId = GetWindowsProductId(),
            CompositeHardwareId = GenerateCompositeHardwareId()
        };
    }
}

public class DeviceInfo
{
    public string? ComputerName { get; init; }
    /// <summary>The Device ID shown in Windows Settings > About</summary>
    public string? WindowsAboutDeviceId { get; init; }
    public string? MachineGuid { get; init; }
    public string? SmbiosUuid { get; init; }
    public string? MotherboardSerial { get; init; }
    public string? BiosSerial { get; init; }
    public string? ProcessorId { get; init; }
    public string? DiskSerial { get; init; }
    public string? WindowsProductId { get; init; }
    public string? CompositeHardwareId { get; init; }
}
