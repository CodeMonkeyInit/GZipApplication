using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using GzipApplication.ChunkedReader;
using GzipApplication.ChunkedWriter;
using GzipApplication.Compressor;
using GzipApplication.Data;

namespace GzipApplication.GZip
{
    /// <summary>
    ///     Gzip compression action.
    /// </summary>
    public class GZipCompressor : BaseGzipAction
    {
        /// <summary>
        ///     Writes header, compresses data and writes it to output.
        /// </summary>
        public override void Execute(Stream input, Stream output)
        {
            WriteHeader(output);
            base.Execute(input, output);
        }

        private void WriteHeader(Stream output)
        {
            output.Write(BitConverter.GetBytes(ArchiveHeader));
        }

        protected override BaseChunkedReader GetReader(Stream inputStream) => new FixLengthChunkedReader(inputStream);

        protected override BaseChunkedWriter GetWriter(Stream output, Func<long?> getChunksCount,
            ManualResetEvent writeCompletedEvent) =>
            new BinaryChunkedDataWriter(output, getChunksCount, writeCompletedEvent);

        public override RentedArray<byte> GetProcessedData(OrderedChunk chunk)
        {
            var length = ArchiveSizeCalculator.CalculateArchiveMaxSizeInBytes(chunk.RentedData.RentedLength);
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