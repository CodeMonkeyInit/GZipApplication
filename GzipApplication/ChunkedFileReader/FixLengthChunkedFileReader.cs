using System;
using System.IO;

namespace GzipApplication.ChunkedFileReader
{
    public class FixLengthChunkedFileReader : IDisposable, IChunkedFileReader
    {
        private readonly FileStream _fileStream;
        private readonly int _chunkSizeInBytes;
        private const int DefaultChunkSizeInBytes = 1_000_000;

        private int _chunksRead = 0;

        public FixLengthChunkedFileReader(FileStream fileStream, int chunkSizeInBytes = DefaultChunkSizeInBytes)
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

        public long? LengthInChunks
        {
            get
            {
                if (!_lengthInChunks.HasValue)
                    _lengthInChunks = (long) Math.Ceiling(((double) _fileStream.Length) / _chunkSizeInBytes);

                return _lengthInChunks.Value;
            }
        }

        private long? _lengthInChunks;

        public bool HasMore => _chunksRead != LengthInChunks;

        public OrderedChunk ReadChunk()
        {
            if (!HasMore)
            {
                throw new InvalidOperationException("All chunks already has been read");
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