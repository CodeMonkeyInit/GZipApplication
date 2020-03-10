using System.Threading;
using GzipApplication.ChunkedWriter;
using GzipApplication.Data;
using GzipApplication.WorkQueue;

namespace GzipApplication.GZip
{
    public class GzipChunkProcessor
    {
        private readonly SemaphoreSlim _readSlotsSemaphore;
        private readonly IOBoundQueue _ioBoundQueue;
        private readonly IChunkedWriter _chunkedWriter;
        private readonly IGzipProcessor _gzipProcessor;

        public GzipChunkProcessor(SemaphoreSlim readSlotsSemaphore,
            IOBoundQueue ioBoundQueue,
            IChunkedWriter chunkedWriter,
            IGzipProcessor gzipProcessor)
        {
            _readSlotsSemaphore = readSlotsSemaphore;
            _ioBoundQueue = ioBoundQueue;
            _chunkedWriter = chunkedWriter;
            _gzipProcessor = gzipProcessor;
        }

        public void Process(OrderedChunk chunk)
        {
            RentedArray<byte> processedData = _gzipProcessor.GetProcessedData(chunk);

            chunk.RentedData.Dispose();

            var orderedChunk = new OrderedChunk
            {
                RentedData = processedData,
                Order = chunk.Order
            };

            var writingFunction = new Function(nameof(IChunkedWriter.WriteOrStoreChunk),
                () => _chunkedWriter.WriteOrStoreChunk(orderedChunk));

            _ioBoundQueue.Enqueue(writingFunction);

            _readSlotsSemaphore.Release();
        }
    }
}