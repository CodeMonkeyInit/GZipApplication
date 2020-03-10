using System;
using System.Buffers;
using GzipApplication.Exceptions;

namespace GzipApplication.Data
{
    /// <summary>
    ///     Array rented from <see cref="ArrayPool{T}"/>
    /// </summary>
    /// <typeparam name="T">Type of data</typeparam>
    public struct RentedArray<T> : IDisposable
    {
        public bool Disposed { get; private set; }

        public T[] Array => Disposed
            ? throw new ArrayDisposedException("Array you are trying to access was returned to ArrayPool")
            : _rentedArray;

        /// <summary>
        /// Returns <see cref="Span{T}"/> which is limited by requested length
        /// </summary>
        public Span<T> AsBoundedSpan => new Span<T>(Array, 0, RentedLength);

        public int RentedLength;

        private readonly ArrayPool<T> _rentedFrom;
        private readonly T[] _rentedArray;

        public RentedArray(T[] rentedArray, int rentedLength, ArrayPool<T> rentedFrom)
        {
            _rentedArray = rentedArray;
            RentedLength = rentedLength;
            _rentedFrom = rentedFrom;
            Disposed = false;
        }

        /// <summary>
        ///     Returns array to pool where it was rented from.
        /// </summary>
        public void Dispose()
        {
            _rentedFrom.Return(_rentedArray);
            Disposed = true;
        }
    }
}