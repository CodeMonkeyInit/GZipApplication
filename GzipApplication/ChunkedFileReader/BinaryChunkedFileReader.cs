using System;
using System.IO;

namespace GzipApplication.ChunkedFileReader
{
    public class BinaryChunkedFileReader : ChunkedFileReader, IDisposable
    {
        private readonly BinaryReader _binaryReader;

        public BinaryChunkedFileReader(FileStream fileStream) : base(fileStream)
        {
            _binaryReader = new BinaryReader(fileStream);
        }

        public override bool HasMore => _binaryReader.BaseStream.Position != _binaryReader.BaseStream.Length;
        public override long? LengthInChunks => HasMore ? default : ChunksRead;

        protected override byte[] ReadBytes()
        {
            var length = _binaryReader.ReadInt32();

            var readBytes = _binaryReader.ReadBytes(length);

            return readBytes;
        }

        public void Dispose()
        {
            _binaryReader.Dispose();
        }
    }
}