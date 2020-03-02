namespace GzipApplication.ChunkedFIleWriter
{
    public interface IChunkedDataWriter
    {
        bool FlushReadyChunks();
    }
}