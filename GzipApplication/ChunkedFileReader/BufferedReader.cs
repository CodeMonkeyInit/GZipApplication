using System;
using System.Threading;
using GzipApplication.Data;
using GzipApplication.ReaderWriter;
using GzipApplication.WorkQueue;

namespace GzipApplication.ChunkedFileReader
{
    public class BufferedReader
    {
        private readonly IChunkedReader _reader;
        private readonly IOBoundQueue _ioBoundQueue;
        private readonly Action<OrderedChunk> _onRead;
        private readonly SemaphoreSlim _readSlotsSemaphore;

        public BufferedReader(IChunkedReader reader, IOBoundQueue ioBoundQueue, Action<OrderedChunk> onRead, SemaphoreSlim readSlotsSemaphore)
        {
            _reader = reader;
            _ioBoundQueue = ioBoundQueue;
            _onRead = onRead;
            _readSlotsSemaphore = readSlotsSemaphore;
        }

        private readonly int _bufferSize = CpuBoundWorkQueue.ParallelWorkMax * 2;
        
        public bool ReadChunks()
        {   
            int readCount = 0;

            //TODO this could lead to threads starvation
            bool BufferIsFull() => readCount >= _bufferSize;

            while (_reader.HasMore && !BufferIsFull() && _readSlotsSemaphore.Wait(TimeSpan.Zero))
            {
                OrderedChunk chunk = _reader.ReadChunk();

                readCount++;

                _onRead(chunk);
            }

            bool isCompleted = !_reader.HasMore;

            if (!isCompleted)
            {
                _ioBoundQueue.Enqueue(new Function
                {
                    Name = nameof(ReadChunks),
                    Payload = ReadChunks
                });
            }

            return isCompleted;
        }
    }
}