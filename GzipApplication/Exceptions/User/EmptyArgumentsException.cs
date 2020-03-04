using System;
using System.Runtime.Serialization;

namespace GzipApplication.Exceptions.User
{
    public class EmptyArgumentsException : UserReadableException
    {
        public EmptyArgumentsException(string message) : base(message)
        {
        }
    }
}