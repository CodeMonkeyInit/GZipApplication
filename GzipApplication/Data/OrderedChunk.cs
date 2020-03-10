using System.Buffers;

namespace GzipApplication.Data
{
    /// <summary>
    ///     Chunk of data which has specific order.
    /// </summary>
    public struct OrderedChunk
    {
        public long Order;

        /// <summary>
        ///     Data rented from <see cref="ArrayPool{T}"/>
        /// </summary>
        public RentedArray<byte> RentedData;
    }
}