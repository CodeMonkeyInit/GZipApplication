using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace GzipApplication
{
    public class ChunkedDataWriter: IDisposable
    {
        private readonly ConcurrentBag<OrderedChunk> _compressedChunks;
        private readonly long _chunksCount;
        private readonly FileStream _fileStream;
        private long _chunksWritten = 0;
        
        public ChunkedDataWriter(string outputFilename, ConcurrentBag<OrderedChunk> compressedCompressedChunks, long chunksCount)
        {
            _compressedChunks = compressedCompressedChunks;
            _chunksCount = chunksCount;
            _fileStream = File.Create(outputFilename);
        }

        public void Dispose()
        {
            _fileStream.Dispose();
        }

        private readonly SortedDictionary<long, OrderedChunk> _sortedDictionary = new SortedDictionary<long, OrderedChunk>();
        
        public bool FlushReadyChunks()
        {
            var compressedChunksCount = _compressedChunks.Count;
            var count = 0;

            while (count < compressedChunksCount && _compressedChunks.TryTake(out OrderedChunk orderedChunk))
            {
                _sortedDictionary.Add(orderedChunk.Order, orderedChunk);
                count++;
            }

            while (_sortedDictionary.ContainsKey(_chunksWritten))
            {
                var orderedChunk = _sortedDictionary[_chunksWritten];
                
                _fileStream.Write(orderedChunk.Data);
                
                _sortedDictionary.Remove(_chunksWritten);
                _chunksWritten++;
            }
            
            _fileStream.Flush(true);

            if (_chunksWritten == _chunksCount)
            {
                return false;
            }

            return true;
        }
    }
}