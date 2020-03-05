using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using GzipApplication.Data;

namespace GzipApplication.ChunkedFIleWriter
{
    public abstract class BaseChunkedWriter : IChunkedDataWriter, IDisposable
    {
        private readonly Func<long?> _getChunksCount;

        private long _chunksWritten = 0;

        private readonly SortedDictionary<long, OrderedChunk> _sortedDictionary =
            new SortedDictionary<long, OrderedChunk>();

        protected BaseChunkedWriter(Func<long?> getChunksCount)
        {
            _getChunksCount = getChunksCount;
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

            return _chunksWritten == chunksCount;
        }

        protected abstract void Write(OrderedChunk chunk);

        protected abstract void Flush();

        public abstract void Dispose();
    }
}