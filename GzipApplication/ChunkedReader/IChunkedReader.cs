using GzipApplication.Data;

namespace GzipApplication.ChunkedReader
{
    /// <summary>
    ///     IO reader that reads data in chunks.
    /// </summary>
    public interface IChunkedReader
    {
        bool HasMore { get; }

        /// <summary>
        ///     Returns length in chunks if it's known or null if it's yet unknown.
        /// </summary>
        public long? LengthInChunks { get; }

        OrderedChunk ReadChunk();
    }
}