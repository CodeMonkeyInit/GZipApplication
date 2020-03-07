using GzipApplication.Data;

namespace GzipApplication.ChunkedFIleWriter
{
    public interface IChunkedWriter
    {
        bool WriteOrAddChunk(OrderedChunk chunk);
    }
}