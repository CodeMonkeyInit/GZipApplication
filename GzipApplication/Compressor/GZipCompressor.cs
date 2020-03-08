using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using GzipApplication.ChunkedReader;
using GzipApplication.ChunkedWriter;
using GzipApplication.Data;

namespace GzipApplication.Compressor
{
    public class GZipCompressor : BaseGzipAction
    {
        protected override BaseChunkedReader GetReader(Stream fileStream) =>
            new FixLengthChunkedReader(fileStream);

        protected override BaseChunkedWriter GetFileWriter(Stream output, Func<long?> getChunksCount, ManualResetEvent writeCompletedEvent) =>
            new BinaryChunkedDataWriter(output, getChunksCount, writeCompletedEvent);

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