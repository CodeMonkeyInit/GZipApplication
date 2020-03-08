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
    public class  GZipDecompressor : BaseGzipAction
    {
        protected override BaseChunkedReader GetReader(Stream stream) =>
            new BinaryChunkedReader(stream);

        protected override BaseChunkedWriter GetFileWriter(Stream outputStream, Func<long?> getChunksCount, ManualResetEvent writeCompetedEvent) =>
            new ChunkWriter(outputStream, getChunksCount, writeCompetedEvent);

        protected override MemoryStream GetProcessedMemoryStream(OrderedChunk chunk)
        {
            using var decompressed = new MemoryStream();

            using var compressedDataStream = new MemoryStream(chunk.Data.ToArray());
            using var gZipStream = new GZipStream(compressedDataStream, CompressionMode.Decompress);

            try
            {
                gZipStream.CopyTo(decompressed);
                gZipStream.Flush();
            }
            catch (InvalidDataException e)
            {
                throw new InvalidArchiveFormatException(UserMessages.ArchiveFormatIsNotSupported, e);
            }

            return decompressed;
        }
    }
}