using System;
using System.Threading;
using GzipApplication.Constants;
using GzipApplication.Data;
using GzipApplication.WorkQueue;

namespace GzipApplication.ChunkedReader.BufferedReader
{
    /// <summary>
    ///     Reads data and fills buffer.
    /// <remarks>Because this reader intended for reading from IO it is not thread safe.</remarks>
    /// </summary>
    public class BufferedReader : IBufferedReader
    {
        private readonly IOBoundQueue _ioBoundQueue;
        private readonly Action<OrderedChunk> _onRead;
        private readonly IChunkedReader _reader;
        private readonly SemaphoreSlim _readSlotsSemaphore;

        public BufferedReader(IChunkedReader reader, IOBoundQueue ioBoundQueue, Action<OrderedChunk> onRead,
            SemaphoreSlim readSlotsSemaphore)
        {
            _reader = reader;
            _ioBoundQueue = ioBoundQueue;
            _onRead = onRead;
            _readSlotsSemaphore = readSlotsSemaphore;
        }

        public bool ReadChunks()
        {
            var readCount = 0;

            bool BufferIsFull() => readCount >= ApplicationConstants.BufferSlots;

            while (_reader.HasMore && !BufferIsFull() && _readSlotsSemaphore.Wait(TimeSpan.Zero))
            {
                var chunk = _reader.ReadChunk();

                readCount++;

                _onRead(chunk);
            }

            var isCompleted = !_reader.HasMore;

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