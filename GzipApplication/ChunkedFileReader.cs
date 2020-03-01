using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace GzipApplication
{
    public class ChunkedFileReader : IDisposable
    {
        private readonly FileStream _fileStream;
        private readonly int _chunkSizeInBytes;
        private const int DefaultChunkSizeInBytes = 1_000_000;

        private int _chunksRead = 0;

        public ChunkedFileReader(FileStream fileStream, int chunkSizeInBytes = DefaultChunkSizeInBytes)
        {
            _fileStream = fileStream ?? throw new ArgumentNullException(nameof(fileStream));
            _chunkSizeInBytes = chunkSizeInBytes;

            if (!fileStream.CanRead)
            {
                throw new ArgumentException("Can't read file");
            }

            if (fileStream.Position != 0)
            {
                throw new ArgumentException("Somebody already tried to read a file!");
            }
        }

        public long LengthInChunks
        {
            get
            {
                if (!_lengthInChunks.HasValue)
                    _lengthInChunks = (long) Math.Ceiling(((double) _fileStream.Length) / _chunkSizeInBytes);

                return _lengthInChunks.Value;
            }
        }

        private long? _lengthInChunks;

        public OrderedChunk? ReadChunk()
        {
            if (_chunksRead == LengthInChunks)
            {
                return null;
            }

            Read(out var readBytes);

            return new OrderedChunk
            {
                Data = readBytes,
                Order = _chunksRead++
            };
        }

        private int Read(out byte[] readBytes)
        {
            long leftToRead = _fileStream.Length - _fileStream.Position;

            var chunkSizeInBytes = leftToRead >= _chunkSizeInBytes
                ? _chunkSizeInBytes
                : (int) leftToRead;

            readBytes = new byte[chunkSizeInBytes];

            if (leftToRead == 0)
                return 0;

            return _fileStream.Read(readBytes, 0, chunkSizeInBytes);
        }
        
        public void Dispose()
        {
            _fileStream.Dispose();
        }
    }
}