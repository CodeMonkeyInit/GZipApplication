using System;
using System.IO;
using GzipApplication.Constants;
using GzipApplication.Data;

namespace GzipApplication.ChunkedReader
{
    /// <summary>
    ///     Reads data in fixed-length chunks. Chunk size gathered from <see cref="ApplicationConstants"/>
    /// <inheritdoc cref="BaseChunkedReader"/>
    /// </summary>
    public class FixLengthChunkedReader : BaseChunkedReader
    {
        private readonly int _chunkSizeInBytes;
        private readonly Stream _fileStream;

        private long? _lengthInChunks;

        public FixLengthChunkedReader(Stream stream)
        {
            _fileStream = stream;
            _chunkSizeInBytes = ApplicationConstants.BufferSizeInBytes;
        }

        public override long? LengthInChunks
        {
            get
            {
                if (!_lengthInChunks.HasValue)
                    _lengthInChunks = (long) Math.Ceiling((double) _fileStream.Length / _chunkSizeInBytes);

                return _lengthInChunks.Value;
            }
        }

        public override bool HasMore => ChunksRead != LengthInChunks;

        protected override RentedArray<byte> ReadBytes()
        {
            var leftToRead = _fileStream.Length - _fileStream.Position;

            var chunkSizeInBytes = leftToRead >= _chunkSizeInBytes
                ? _chunkSizeInBytes
                : (int) leftToRead;

            var rentedArray = GzipArrayPool.SharedBytesPool.Rent(chunkSizeInBytes);

            _fileStream.Read(rentedArray, 0, chunkSizeInBytes);

            return new RentedArray<byte>(rentedArray, chunkSizeInBytes, GzipArrayPool.SharedBytesPool);
        }

        public override void Dispose()
        {
            _fileStream.Dispose();
        }
    }
}