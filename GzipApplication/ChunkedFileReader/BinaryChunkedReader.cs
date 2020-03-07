using System;
using System.IO;
using GzipApplication.Constants;
using GzipApplication.Exceptions.User;

namespace GzipApplication.ChunkedFileReader
{
    public class BinaryChunkedReader : BaseChunkedReader, IDisposable
    {
        private readonly BinaryReader _binaryReader;

        public BinaryChunkedReader(FileStream fileStream) : base(fileStream)
        {
            _binaryReader = new BinaryReader(fileStream);
        }

        public override bool HasMore => _binaryReader.BaseStream.Position != _binaryReader.BaseStream.Length;
        public override long? LengthInChunks => HasMore ? default : ChunksRead;

        protected override byte[] ReadBytes()
        {
            var length = _binaryReader.ReadInt32();

            if (length < 0)
            {
                throw new InvalidArchiveFormatException(UserMessages.ArchiveFormatIsNotSupported);
            }

            var readBytes = _binaryReader.ReadBytes(length);

            return readBytes;
        }

        public override void Dispose()
        {
            _binaryReader.Dispose();
        }
    }
}