using GzipApplication.Files;

namespace GZipApplication.Tests
{
    public class InMemoryFileService : IFileService
    {
        public bool IsFilesOnDifferentDrives(string inputFilename, string outputFilename)
        {
            return true;
        }
    }
}