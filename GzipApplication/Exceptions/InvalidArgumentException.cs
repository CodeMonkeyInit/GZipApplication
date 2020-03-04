using System;
using System.Runtime.Serialization;

namespace GzipApplication.Exceptions
{
    public class InvalidArgumentException : UserReadableException
    {
        public InvalidArgumentException()
        {
        }

        protected InvalidArgumentException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public InvalidArgumentException(string message) : base(message)
        {
        }

        public InvalidArgumentException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}