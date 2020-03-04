using System;
using System.Collections.Concurrent;
using System.IO;
using GzipApplication.Data;

namespace GzipApplication.ChunkedFIleWriter
{
    public class ChunkWriter : BaseChunkedWriter
    {
        private readonly FileStream _fileStream;

        public ChunkWriter(string filename, ConcurrentBag<OrderedChunk> chunks, Func<long?> getChunksCount) :
            base(chunks, getChunksCount)
        {
            _fileStream = File.Create(filename);
        }

        protected override void Write(OrderedChunk chunk)
        {
            _fileStream.Write(chunk.Data.Span);
        }

        protected override void Flush()
        {
            _fileStream.Flush();
        }

        public override void Dispose()
        {
            _fileStream.Dispose();
        }
    }
}