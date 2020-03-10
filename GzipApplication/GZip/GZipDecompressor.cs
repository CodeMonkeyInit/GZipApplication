using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using GzipApplication.ChunkedReader;
using GzipApplication.ChunkedWriter;
using GzipApplication.Constants;
using GzipApplication.Data;
using GzipApplication.Exceptions.User;

namespace GzipApplication.GZip
{
    /// <summary>
    ///     Gzip decompression action
    /// </summary>
    public class GZipDecompressor : BaseGzipAction, IGzipProcessor
    {
        /// <summary>
        ///     Validates header, decompresses data and writes to output.
        /// </summary>
        public override void Execute(Stream input, Stream output)
        {
            ValidateHeader(input);
            base.Execute(input, output);
        }

        private void ValidateHeader(Stream input)
        {
            Span<byte> bytes = stackalloc byte[sizeof(long)];

            input.Read(bytes);

            var supposedHeader = BitConverter.ToInt64(bytes);

            if (supposedHeader != ArchiveHeader)
                throw new InvalidArchiveFormatException(UserMessages.ArchiveFormatIsNotSupported);
        }

        protected override BaseChunkedReader GetReader(Stream inputStream) => new BinaryChunkedReader(inputStream);

        protected override BaseChunkedWriter GetWriter(Stream outputStream, Func<long?> getChunksCount,
            ManualResetEvent writeCompetedEvent) =>
            new ChunkWriter(outputStream, getChunksCount, writeCompetedEvent);

        public override RentedArray<byte> GetProcessedData(OrderedChunk chunk)
        {
            var length = ApplicationConstants.BufferSizeInBytes;
            var rentedBytes = GzipArrayPool.SharedBytesPool.Rent(length);

            using var decompressedStream = new MemoryStream(rentedBytes, 0, length, true);
            decompressedStream.SetLength(0);

            using var compressedDataStream = new MemoryStream(chunk.RentedData.Array, 0, chunk.RentedData.RentedLength);
            using var gZipStream = new GZipStream(compressedDataStream, CompressionMode.Decompress);

            try
            {
                gZipStream.CopyTo(decompressedStream);
                gZipStream.Flush();
            }
            catch (InvalidDataException e)
            {
                throw new InvalidArchiveFormatException(UserMessages.ArchiveFormatIsNotSupported, e);
            }

            return new RentedArray<byte>(rentedBytes, (int) decompressedStream.Length, GzipArrayPool.SharedBytesPool);
        }
    }
}