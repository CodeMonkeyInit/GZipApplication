using System.Collections.Generic;
using System.Linq;
using System.Management;

namespace GzipApplication.Files.Windows
{
    public class DriveManagementObject
    {
        private readonly ManagementObject _drive;

        public DriveManagementObject(ManagementObject drive)
        {
            _drive = drive;
        }

        public string Name => _drive.GetName();

        public IEnumerable<ManagementObject> Partitions => _drive.GetPartitions();

        public IEnumerable<ManagementObject> LogicalDrives =>
            Partitions.SelectMany(partition => WindowsManagementObjectExtensions.GetLogicalDrives(partition));

        public bool HasLogicalDrive(char driveLetter) =>
            LogicalDrives.Any(drive =>
                char.ToLower(drive.GetName()[0]) == char.ToLower(driveLetter));
    }
}