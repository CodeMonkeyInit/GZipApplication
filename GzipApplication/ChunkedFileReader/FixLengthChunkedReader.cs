using System;
using System.IO;

namespace GzipApplication.ChunkedFileReader
{
    public class FixLengthChunkedReader : BaseChunkedReader, IDisposable
    {
        private readonly FileStream _fileStream;
        private readonly int _chunkSizeInBytes;
        private const int DefaultChunkSizeInBytes = 1_000_000;

        public FixLengthChunkedReader(FileStream fileStream, int chunkSizeInBytes = DefaultChunkSizeInBytes) :
            base(fileStream)
        {
            _fileStream = fileStream ?? throw new ArgumentNullException(nameof(fileStream));
            _chunkSizeInBytes = chunkSizeInBytes;
        }

        public override long? LengthInChunks
        {
            get
            {
                if (!_lengthInChunks.HasValue)
                    _lengthInChunks = (long) Math.Ceiling(((double) _fileStream.Length) / _chunkSizeInBytes);

                return _lengthInChunks.Value;
            }
        }

        private long? _lengthInChunks;

        public override bool HasMore => ChunksRead != LengthInChunks;

        protected override byte[] ReadBytes()
        {
            long leftToRead = _fileStream.Length - _fileStream.Position;

            var chunkSizeInBytes = leftToRead >= _chunkSizeInBytes
                ? _chunkSizeInBytes
                : (int) leftToRead;

            var readBytes = new byte[chunkSizeInBytes];

            _fileStream.Read(readBytes, 0, chunkSizeInBytes);

            return readBytes;
        }

        public override void Dispose()
        {
            _fileStream.Dispose();
        }
    }
}