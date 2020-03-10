using System;
using System.Buffers;
using GzipApplication.Data;

namespace GzipApplication.ChunkedReader
{
    /// <summary>
    ///     Reader that consumes data by chunks.
    ///     <remarks>Intended for IO usage so it's not thread safe.</remarks>
    /// </summary>
    public abstract class BaseChunkedReader : IChunkedReader, IDisposable
    {
        public abstract bool HasMore { get; }

        public abstract long? LengthInChunks { get; }

        protected long ChunksRead;

        /// <summary>
        ///     Reads one chunk of data.
        /// </summary>
        /// <returns>Chunk of data with its relative order</returns>
        /// <exception cref="InvalidOperationException">Throws if all chunks were read.</exception>
        public OrderedChunk ReadChunk()
        {
            if (!HasMore)
                throw new InvalidOperationException("All chunks were already read.");

            var readBytes = ReadBytes();

            return new OrderedChunk
            {
                RentedData = readBytes,
                Order = ChunksRead++
            };
        }

        /// <summary>
        ///     Reads bytes
        /// </summary>
        /// <returns>Array rented from <see cref="ArrayPool{T}"/></returns>
        protected abstract RentedArray<byte> ReadBytes();

        public abstract void Dispose();
    }
}