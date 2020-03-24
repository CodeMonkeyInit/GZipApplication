namespace GzipApplication.Data
{
    public interface IThreadSafeQueue<T>
    {
        bool Enqueue(T value);

        bool TryDequeue(out T value);
    }
}