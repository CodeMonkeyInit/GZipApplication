using System;
using System.Runtime.Serialization;

namespace GzipApplication.Exceptions.User
{
    public class InvalidFilePath: UserReadableException
    {
        public InvalidFilePath(string message) : base(message)
        {
        }
    }
}