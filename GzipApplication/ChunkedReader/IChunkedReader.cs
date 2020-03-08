using GzipApplication.Data;

namespace GzipApplication.ChunkedReader
{
    public interface IChunkedReader
    {
        bool HasMore { get; }

        public long? LengthInChunks { get; }
        
        OrderedChunk ReadChunk();
    }
}