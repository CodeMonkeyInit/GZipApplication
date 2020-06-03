using System;
using System.IO;
using System.Linq;
using GzipApplication.Exceptions.User;

namespace GzipApplication.Files.Windows
{
    public class WindowsFileService : IFileService
    {
        public bool IsFilesOnDifferentDrives(string inputFilename, string outputFilename)
        {
            if (string.IsNullOrWhiteSpace(inputFilename))
                throw new InvalidArgumentException(inputFilename);

            if (string.IsNullOrWhiteSpace(outputFilename))
                throw new InvalidArgumentException(outputFilename);

            try
            {
                return IsFilesOnDifferentDrivesImpl(inputFilename, outputFilename);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool IsFilesOnDifferentDrivesImpl(string inputFilename, string outputFilename)
        {
            var inputFileInfo = new FileInfo(inputFilename);
            var outputFileInfo = new FileInfo(outputFilename);

            var inputFileDriveInfo = new DriveInfo(inputFileInfo.Directory.FullName);
            var outputFileDriveInfo = new DriveInfo(outputFileInfo.Directory.FullName);

            const string wmiDrivesQuery = "select * from Win32_DiskDrive";

            var drives = wmiDrivesQuery
                .GetManagementObjects()
                .Select(drive => new DriveManagementObject(drive))
                .ToArray();

            var inputFileDrive = drives.FirstOrDefault(drive => drive.HasLogicalDrive(inputFileDriveInfo.Name[0]));
            var outputFileDrive = drives.FirstOrDefault(drive => drive.HasLogicalDrive(outputFileDriveInfo.Name[0]));

            var filesAreOnDifferentDrives = inputFileDrive != outputFileDrive;

            return filesAreOnDifferentDrives;
        }
    }
}