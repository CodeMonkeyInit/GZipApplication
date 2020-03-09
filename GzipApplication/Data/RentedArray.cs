using System;
using System.Buffers;
using GzipApplication.Exceptions;

namespace GzipApplication.Data
{
    public struct RentedArray<T> : IDisposable
    {
        public bool Disposed { get; private set; }

        public T[] Array => Disposed
            ? throw new ArrayDisposedException("Array you are trying to access was returned to ArrayPool")
            : _rentedArray;

        public Span<T> AsSpan => new Span<T>(Array, 0, RentedLength);

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

        public void Dispose()
        {
            _rentedFrom.Return(_rentedArray);
            Disposed = true;
        }
    }
}