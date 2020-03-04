using System;

namespace GzipApplication.Exceptions
{
    public class InvalidArchiveFormatException: UserReadableException
    {
        public InvalidArchiveFormatException(string message) : base(message)
        {
        }

        public InvalidArchiveFormatException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}