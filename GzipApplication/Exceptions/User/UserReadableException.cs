using System;

namespace GzipApplication.Exceptions.User
{
    /// <summary>
    ///     Exception that should be readable by user.
    /// </summary>
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