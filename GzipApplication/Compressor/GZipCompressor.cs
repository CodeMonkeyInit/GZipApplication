using System;
using System.IO;
using System.IO.Compression;
using GzipApplication.ChunkedFileReader;
using GzipApplication.ChunkedFIleWriter;
using GzipApplication.Data;

namespace GzipApplication.Compressor
{
    public class GZipCompressor : BaseGzipAction
    {
        protected override BaseChunkedFileReader GetFileReader(FileStream fileStream) =>
            new FixLengthChunkedFileReader(fileStream);

        protected override BaseChunkedWriter GetFileWriter(string filename, Func<long?> getChunksCount) =>
            new BinaryChunkedDataWriter(filename, getChunksCount);

        protected override MemoryStream GetProcessedMemoryStream(OrderedChunk chunk)
        {
            using var memoryStream = new MemoryStream();
            using var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress);

            gZipStream.Write(chunk.Data.Span);
            gZipStream.Flush();

            return memoryStream;
        }
    }
}