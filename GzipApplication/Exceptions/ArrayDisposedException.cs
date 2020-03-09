using System;

namespace GzipApplication.Exceptions
{
    public class ArrayDisposedException : Exception
    {
        public ArrayDisposedException(string? message) : base(message)
        {
        }
    }
}