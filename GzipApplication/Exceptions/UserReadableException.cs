using System;
using System.Runtime.Serialization;

namespace GzipApplication.Exceptions
{
    
    public class UserReadableException : Exception
    {
        public UserReadableException()
        {
        }

        protected UserReadableException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public UserReadableException(string message) : base(message)
        {
        }

        public UserReadableException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}