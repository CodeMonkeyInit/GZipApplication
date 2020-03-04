using System;

namespace GzipApplication.Exceptions.User
{
    public class InvalidArchiveFormatException: UserReadableException
    {
        public InvalidArchiveFormatException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}