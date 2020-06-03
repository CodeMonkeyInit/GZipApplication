using System.Runtime.InteropServices;
using GzipApplication.Files.Windows;

namespace GzipApplication.Files
{
    public class FileServiceFactory
    {
        public static IFileService GetFileService()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new WindowsFileService();
            }

            return new UnixFileService();
        }
    }
}