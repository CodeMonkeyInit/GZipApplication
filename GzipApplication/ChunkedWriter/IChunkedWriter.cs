using System.IO;
using GzipApplication.Data;

namespace GzipApplication.ChunkedWriter
{
    /// <summary>
    ///     Writes ordered data in chunks.
    /// </summary>
    public interface IChunkedWriter
    {
        /// <summary>
        ///     Writes ordered data to underlying <see cref="Stream"/> or stores it for later if ordering is incorrect.
        /// </summary>
        /// <param name="chunk">Ordered chunk of data.</param>
        /// <returns>true if every chunk was written or false if otherwise.</returns>
        bool WriteOrStoreChunk(OrderedChunk chunk);
    }
}