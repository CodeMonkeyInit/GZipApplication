namespace GzipApplication.Compressor
{
    public interface IGZipCompressor
    {
        void Compress(string inputFilename, string outputFilename);
        void Decompress(string inputFilename, string outputFilename);
    }
}