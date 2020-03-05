using System;
using System.IO;
using System.IO.Compression;
using GzipApplication.ChunkedFileReader;
using GzipApplication.ChunkedFIleWriter;
using GzipApplication.Constants;
using GzipApplication.Data;
using GzipApplication.Exceptions.User;

namespace GzipApplication.Compressor
{
    public class GZipDecompressor : BaseGzipAction
    {
        protected override BaseChunkedFileReader GetFileReader(FileStream fileStream) =>
            new BinaryChunkedFileReader(fileStream);

        protected override BaseChunkedWriter GetFileWriter(string filename, Func<long?> getChunksCount) =>
            new ChunkWriter(filename, getChunksCount);

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