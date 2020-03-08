using GzipApplication.Data;

namespace GzipApplication.ChunkedWriter
{
    public interface IChunkedWriter
    {
        bool WriteOrAddChunk(OrderedChunk chunk);
    }
}