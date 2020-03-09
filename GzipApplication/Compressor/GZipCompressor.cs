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
        protected override BaseChunkedReader GetReader(Stream fileStream)
        {
            return new FixLengthChunkedReader(fileStream);
        }

        protected override BaseChunkedWriter GetFileWriter(Stream output, Func<long?> getChunksCount,
            ManualResetEvent writeCompletedEvent)
        {
            return new BinaryChunkedDataWriter(output, getChunksCount, writeCompletedEvent);
        }

        protected override RentedArray<byte> GetProcessedData(OrderedChunk chunk)
        {
            var length = CalculateArchiveMaxSize(chunk.RentedData.RentedLength);
            var rentedArray = GzipArrayPool.SharedBytesPool.Rent(length);

            using var compressedStream = new MemoryStream(rentedArray, 0, length, true);
            compressedStream.SetLength(0);

            using var gZipStream = new GZipStream(compressedStream, CompressionMode.Compress);

            gZipStream.Write(chunk.RentedData.AsBoundedSpan);
            gZipStream.Flush();

            return new RentedArray<byte>(rentedArray, (int) compressedStream.Length, GzipArrayPool.SharedBytesPool);
        }
    }
}