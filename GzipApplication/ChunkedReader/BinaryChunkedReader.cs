using System.IO;
using GzipApplication.Data;

namespace GzipApplication.ChunkedReader
{
    /// <summary>
    ///     Uses <see cref="BinaryReader"/> to consume data.
    /// <inheritdoc cref="BaseChunkedReader"/>
    /// </summary>
    public class BinaryChunkedReader : BaseChunkedReader
    {
        private readonly BinaryReader _binaryReader;

        public BinaryChunkedReader(Stream stream)
        {
            _binaryReader = new BinaryReader(stream);
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

            var rentedArray = GzipArrayPool.SharedBytesPool.Rent(length);

            _binaryReader.Read(rentedArray, 0, length);

            return new RentedArray<byte>(rentedArray, length, GzipArrayPool.SharedBytesPool);
        }
    }
}