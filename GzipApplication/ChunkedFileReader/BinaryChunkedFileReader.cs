using System;
using System.IO;

namespace GzipApplication.ChunkedFileReader
{
    public class BinaryChunkedFileReader: IDisposable, IChunkedFileReader
    {
        private readonly BinaryReader _binaryReader;

        public BinaryChunkedFileReader(FileStream fileStream)
        {
            _binaryReader = new BinaryReader(fileStream);
        }

        public bool HasMore => _binaryReader.BaseStream.Position != _binaryReader.BaseStream.Length;
        public long? LengthInChunks => HasMore ? default : _chunksRead;

        private long _chunksRead = 0;
        public OrderedChunk ReadChunk()
        {
            if (!HasMore)
            {
                throw new InvalidOperationException("All chunks are already read");
            }
            
            var length = _binaryReader.ReadInt32();

            var readBytes = _binaryReader.ReadBytes(length);

            return new OrderedChunk
            {
                Data = readBytes,
                Order = _chunksRead++
            };
        }
        
        public void Dispose()
        {
            _binaryReader.Dispose();
        }
    }
}