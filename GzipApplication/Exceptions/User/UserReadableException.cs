using System;
using System.Runtime.Serialization;

namespace GzipApplication.Exceptions.User
{
    
    public class UserReadableException : Exception
    {
        public UserReadableException(string message) : base(message)
        {
        }

        public UserReadableException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}