namespace GzipApplication.Exceptions.User
{
    public class UnreadableFileException: UserReadableException
    {
        public UnreadableFileException(string message) : base(message)
        {
        }
    }
}