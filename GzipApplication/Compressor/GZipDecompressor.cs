using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using GzipApplication.ChunkedReader;
using GzipApplication.ChunkedWriter;
using GzipApplication.Constants;
using GzipApplication.Data;
using GzipApplication.Exceptions.User;

namespace GzipApplication.Compressor
{
    public class GZipDecompressor : BaseGzipAction
    {
        protected override BaseChunkedReader GetReader(Stream stream)
        {
            return new BinaryChunkedReader(stream);
        }

        protected override BaseChunkedWriter GetFileWriter(Stream outputStream, Func<long?> getChunksCount,
            ManualResetEvent writeCompetedEvent)
        {
            return new ChunkWriter(outputStream, getChunksCount, writeCompetedEvent);
        }

        protected override RentedArray<byte> GetProcessedData(OrderedChunk chunk)
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