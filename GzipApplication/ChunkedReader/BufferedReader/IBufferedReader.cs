namespace GzipApplication.ChunkedReader.BufferedReader
{
    public interface IBufferedReader
    {
        /// <summary>
        ///     Read chunks of data using reader until buffer is full or no more read slots are available.
        /// </summary>
        /// <returns>true of all chunks were read or false if there is more chunks to read.</returns>
        bool ReadChunks();
    }
}