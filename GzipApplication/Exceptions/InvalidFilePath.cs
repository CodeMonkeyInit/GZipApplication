using System;
using System.Runtime.Serialization;

namespace GzipApplication.Exceptions
{
    public class InvalidFilePath: UserReadableException
    {
        public InvalidFilePath()
        {
        }

        protected InvalidFilePath(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public InvalidFilePath(string message) : base(message)
        {
        }

        public InvalidFilePath(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}