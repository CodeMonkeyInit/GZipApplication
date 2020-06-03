namespace GzipApplication.Files
{
    public interface IFileService
    {
        bool IsFilesOnDifferentDrives(string inputFilename, string outputFilename);
    }
}