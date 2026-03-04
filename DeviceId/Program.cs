using System.Runtime.InteropServices;
using DeviceId;

Console.WriteLine("=== Windows Device ID Information ===\n");

if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    Console.WriteLine("This tool is designed for Windows only.");
    Console.WriteLine($"Current OS: {RuntimeInformation.OSDescription}");
    Console.WriteLine("\nOn Windows, this tool retrieves:");
    Console.WriteLine("  - Device ID (shown in Settings > About)");
    Console.WriteLine("  - MachineGuid (from registry)");
    Console.WriteLine("  - SMBIOS UUID (hardware-based)");
    Console.WriteLine("  - Motherboard Serial");
    Console.WriteLine("  - BIOS Serial");
    Console.WriteLine("  - Processor ID");
    Console.WriteLine("  - Disk Serial");
    Console.WriteLine("  - Composite Hardware ID (SHA256 hash of above)");
    return;
}

var info = DeviceIdHelper.GetDeviceInfo();

Console.WriteLine($"Computer Name:        {info.ComputerName}");
Console.WriteLine();

Console.WriteLine("--- Device ID (Settings > System > About) ---");
Console.WriteLine($"Device ID:            {info.WindowsAboutDeviceId ?? "N/A"}");
Console.WriteLine();

Console.WriteLine("--- Software-based IDs (change on OS reinstall) ---");
Console.WriteLine($"MachineGuid:          {info.MachineGuid ?? "N/A"}");
Console.WriteLine($"Windows Product ID:   {info.WindowsProductId ?? "N/A"}");
Console.WriteLine();

Console.WriteLine("--- Hardware-based IDs (persist across reinstalls) ---");
Console.WriteLine($"SMBIOS UUID:          {info.SmbiosUuid ?? "N/A"}");
Console.WriteLine($"Motherboard Serial:   {info.MotherboardSerial ?? "N/A"}");
Console.WriteLine($"BIOS Serial:          {info.BiosSerial ?? "N/A"}");
Console.WriteLine($"Processor ID:         {info.ProcessorId ?? "N/A"}");
Console.WriteLine($"Disk Serial:          {info.DiskSerial ?? "N/A"}");
Console.WriteLine();

Console.WriteLine("--- Composite ID (SHA256 hash of hardware IDs) ---");
Console.WriteLine($"Composite Hardware ID: {info.CompositeHardwareId ?? "N/A"}");
Console.WriteLine();

Console.WriteLine("--- Recommended Usage ---");
Console.WriteLine("  - For licensing: Use MachineGuid (easy) or CompositeHardwareId (more stable)");
Console.WriteLine("  - For hardware tracking: Use SMBIOS UUID");
Console.WriteLine("  - For device correlation: Combine multiple IDs");
