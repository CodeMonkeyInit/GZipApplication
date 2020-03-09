using System.Buffers;

namespace GzipApplication.Data
{
    public class GzipArrayPool
    {
        /// <summary>
        ///     Wrap around standard ArrayPool in case we want to go fully custom and increase max array size.
        ///     <remarks>Max array size for ArrayPool is 1 048 576 bytes</remarks>
        /// </summary>
        public static readonly ArrayPool<byte> SharedBytesPool = ArrayPool<byte>.Shared;
    }
}