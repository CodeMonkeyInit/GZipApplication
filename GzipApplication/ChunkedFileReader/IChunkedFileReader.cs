namespace GzipApplication.ChunkedFileReader
{
    public interface IChunkedFileReader
    {
        bool HasMore { get; }

        public long? LengthInChunks { get; }
        
        OrderedChunk ReadChunk();
    }
}