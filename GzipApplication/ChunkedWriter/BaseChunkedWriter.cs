using System;
using System.Collections.Generic;
using System.Threading;
using GzipApplication.Data;

namespace GzipApplication.ChunkedWriter
{
    public abstract class BaseChunkedWriter : IChunkedWriter, IDisposable
    {
        private readonly Func<long?> _getChunksCount;
        private readonly ManualResetEvent _writeCompletedEvent;

        private long _chunksWritten = 0;

        private readonly SortedDictionary<long, OrderedChunk> _sortedDictionary =
            new SortedDictionary<long, OrderedChunk>();

        protected BaseChunkedWriter(Func<long?> getChunksCount, ManualResetEvent writeCompletedEvent)
        {
            _getChunksCount = getChunksCount;
            _writeCompletedEvent = writeCompletedEvent;
        }

        public bool WriteOrAddChunk(OrderedChunk chunk)
        {
            long? chunksCount = _getChunksCount();
            
            _sortedDictionary.Add(chunk.Order, chunk);

            while (_sortedDictionary.ContainsKey(_chunksWritten))
            {
                var orderedChunk = _sortedDictionary[_chunksWritten];

                Write(orderedChunk);

                _sortedDictionary.Remove(_chunksWritten);
                _chunksWritten++;
            }

            Flush();

            var writeEnded = _chunksWritten == chunksCount;
            
            if (writeEnded)
                _writeCompletedEvent.Set();

            return writeEnded;
        }

        protected abstract void Write(OrderedChunk chunk);

        protected abstract void Flush();

        public abstract void Dispose();
    }
}