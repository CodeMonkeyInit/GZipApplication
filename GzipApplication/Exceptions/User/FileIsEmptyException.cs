namespace GzipApplication.Exceptions.User
{
    public class FileIsEmptyException: UserReadableException
    {
        public FileIsEmptyException(string message) : base(message)
        {
        }
    }
}