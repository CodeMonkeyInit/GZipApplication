using System.Collections.Generic;
using System.Linq;
using System.Management;

namespace GzipApplication.Files.Windows
{
    public static class WindowsManagementObjectExtensions
    {
        public static IEnumerable<ManagementObject> GetManagementObjects(this string query) =>
            new ManagementObjectSearcher(query).Get().OfType<ManagementObject>();

        public static string GetName(this ManagementObject managementObject) =>
            managementObject.Properties["Name"]?.Value?.ToString() ?? string.Empty;

        public static IEnumerable<ManagementObject> GetPartitions(this ManagementObject drive) =>
            $"associators of {{{drive.Path.RelativePath}}} where AssocClass = Win32_DiskDriveToDiskPartition"
                .GetManagementObjects();

        public static IEnumerable<ManagementObject> GetLogicalDrives(this ManagementObject partition) =>
            $"associators of {{{partition.Path.RelativePath}}} where AssocClass = Win32_LogicalDiskToPartition"
                .GetManagementObjects();
    }
}