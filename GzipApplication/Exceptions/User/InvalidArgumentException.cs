using System;
using System.Runtime.Serialization;

namespace GzipApplication.Exceptions.User
{
    public class InvalidArgumentException : UserReadableException
    {
        public InvalidArgumentException(string message) : base(message)
        {
        }
    }
}