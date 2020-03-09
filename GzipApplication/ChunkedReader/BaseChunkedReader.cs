using System;
using System.IO;
using GzipApplication.Data;
using GzipApplication.Exceptions.User;

namespace GzipApplication.ChunkedReader
{
    public abstract class BaseChunkedReader : IChunkedReader, IDisposable
    {
        protected long ChunksRead;

        protected BaseChunkedReader(Stream fileStream)
        {
            ValidateStream(fileStream);
        }

        public abstract bool HasMore { get; }
        public abstract long? LengthInChunks { get; }

        public OrderedChunk ReadChunk()
        {
            if (!HasMore) throw new InvalidOperationException("All chunks were already read.");

            var readBytes = ReadBytes();

            return new OrderedChunk
            {
                RentedData = readBytes,
                Order = ChunksRead++
            };
        }

        public abstract void Dispose();

        private void ValidateStream(Stream stream)
        {
            if (stream.Position != 0) throw new InvalidArgumentException("Other thread already tried to read a file!");
        }

        protected abstract RentedArray<byte> ReadBytes();
    }
}