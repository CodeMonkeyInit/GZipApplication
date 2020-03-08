using System;
using System.IO;
using GzipApplication.Data;
using GzipApplication.Exceptions.User;

namespace GzipApplication.ChunkedReader
{
    public abstract class BaseChunkedReader : IChunkedReader, IDisposable
    {
        public abstract bool HasMore { get; }
        public abstract long? LengthInChunks { get; }
        
        protected long ChunksRead = 0;
        
        public OrderedChunk ReadChunk()
        {
            if (!HasMore)
            {
                throw new InvalidOperationException("All chunks were already read.");
            }

            var readBytes = ReadBytes();

            return new OrderedChunk
            {
                Data = readBytes,
                Order = ChunksRead++
            };
        }

        protected BaseChunkedReader(Stream fileStream)
        {
            ValidateStream(fileStream);
        }

        private void ValidateStream(Stream stream)
        {
            if (stream.Position != 0)
            {
                throw new InvalidArgumentException("Other thread already tried to read a file!");
            }
        }

        protected abstract byte[] ReadBytes();

        public abstract void Dispose();
    }
}