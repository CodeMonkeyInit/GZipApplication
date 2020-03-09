using System;
using System.IO;
using GzipApplication.Constants;
using GzipApplication.Data;
using GzipApplication.Exceptions.User;

namespace GzipApplication.ChunkedReader
{
    public class BinaryChunkedReader : BaseChunkedReader, IDisposable
    {
        private readonly BinaryReader _binaryReader;

        public BinaryChunkedReader(Stream fileStream) : base(fileStream)
        {
            _binaryReader = new BinaryReader(fileStream);
        }

        public override bool HasMore => _binaryReader.BaseStream.Position != _binaryReader.BaseStream.Length;
        public override long? LengthInChunks => HasMore ? default : ChunksRead;

        public override void Dispose()
        {
            _binaryReader.Dispose();
        }

        protected override RentedArray<byte> ReadBytes()
        {
            var length = _binaryReader.ReadInt32();

            if (length < 0) throw new InvalidArchiveFormatException(UserMessages.ArchiveFormatIsNotSupported);

            var rentedArray = GzipArrayPool.SharedBytesPool.Rent(length);

            _binaryReader.Read(rentedArray, 0, length);

            return new RentedArray<byte>(rentedArray, length, GzipArrayPool.SharedBytesPool);
        }
    }
}