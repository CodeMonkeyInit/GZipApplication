using System;
using System.Collections.Generic;
using System.Threading;
using GzipApplication.Data;

namespace GzipApplication.ChunkedWriter
{
    public abstract class BaseChunkedWriter : IChunkedWriter, IDisposable
    {
        private readonly Func<long?> _getChunksCount;

        private readonly SortedDictionary<long, OrderedChunk> _sortedDictionary =
            new SortedDictionary<long, OrderedChunk>();

        private readonly ManualResetEvent _writeCompletedEvent;

        private long _chunksWritten;

        protected BaseChunkedWriter(Func<long?> getChunksCount, ManualResetEvent writeCompletedEvent)
        {
            _getChunksCount = getChunksCount;
            _writeCompletedEvent = writeCompletedEvent;
        }

        public bool WriteOrAddChunk(OrderedChunk chunk)
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

        protected abstract void Write(OrderedChunk chunk);

        protected abstract void Flush();
    }
}