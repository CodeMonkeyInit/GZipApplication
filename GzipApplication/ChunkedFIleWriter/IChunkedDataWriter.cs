using GzipApplication.Data;

namespace GzipApplication.ChunkedFIleWriter
{
    public interface IChunkedDataWriter
    {
        bool WriteOrAddChunk(OrderedChunk chunk);
    }
}