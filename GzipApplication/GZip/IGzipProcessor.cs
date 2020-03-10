using System.Buffers;
using GzipApplication.Data;

namespace GzipApplication.GZip
{
    public interface IGzipProcessor
    {
        /// <summary>
        ///     Processes <see cref="OrderedChunk"/> data.
        /// </summary>
        /// <returns>Array rented from <see cref="ArrayPool{T}"/></returns>
        RentedArray<byte> GetProcessedData(OrderedChunk chunk);
    }
}