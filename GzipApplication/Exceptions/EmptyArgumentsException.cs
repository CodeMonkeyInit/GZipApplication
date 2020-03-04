using System;
using System.Runtime.Serialization;

namespace GzipApplication.Exceptions
{
    public class EmptyArgumentsException : UserReadableException
    {
        public EmptyArgumentsException()
        {
        }

        protected EmptyArgumentsException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public EmptyArgumentsException(string message) : base(message)
        {
        }

        public EmptyArgumentsException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}