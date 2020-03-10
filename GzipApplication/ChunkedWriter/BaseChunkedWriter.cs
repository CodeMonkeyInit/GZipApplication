using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using GzipApplication.Data;

namespace GzipApplication.ChunkedWriter
{
    /// <summary>
    /// <inheritdoc cref="IChunkedWriter"/>
    /// <remarks>Intended for usage with IO streams so not thread-safe.</remarks>
    /// </summary>
    public abstract class BaseChunkedWriter : IChunkedWriter, IDisposable
    {
        private readonly Func<long?> _getChunksCount;

        private readonly SortedDictionary<long, OrderedChunk> _sortedDictionary =
            new SortedDictionary<long, OrderedChunk>();

        private readonly ManualResetEvent _writeCompletedEvent;

        private long _chunksWritten;

        /// <param name="getChunksCount"><see cref="Func{TResult}"/> that returns amount of chunks to be written</param>
        /// <param name="writeCompletedEvent">Event handle to signal that all chunks was written</param>
        protected BaseChunkedWriter(Func<long?> getChunksCount, ManualResetEvent writeCompletedEvent)
        {
            _getChunksCount = getChunksCount;
            _writeCompletedEvent = writeCompletedEvent;
        }

        public bool WriteOrStoreChunk(OrderedChunk chunk)
        {
            var chunksCount = _getChunksCount();

            _sortedDictionary.Add(chunk.Order, chunk);

            while (_sortedDictionary.ContainsKey(_chunksWritten))
            {
                var orderedChunk = _sortedDictionary[_chunksWritten];

                Write(orderedChunk);

                orderedChunk.RentedData.Dispose();

                _sortedDictionary.Remove(_chunksWritten);
                _chunksWritten++;
            }

            Flush();

            var writeEnded = _chunksWritten == chunksCount;

            if (writeEnded)
                _writeCompletedEvent.Set();

            return writeEnded;
        }

        public abstract void Dispose();

        /// <summary>
        ///     Writes chunk to underlying <see cref="Stream"/>
        /// </summary>
        protected abstract void Write(OrderedChunk chunk);

        protected abstract void Flush();
    }
}