using GzipApplication.Data;

namespace GzipApplication.ChunkedFileReader
{
    public interface IChunkedReader
    {
        bool HasMore { get; }

        public long? LengthInChunks { get; }
        
        OrderedChunk ReadChunk();
    }
}