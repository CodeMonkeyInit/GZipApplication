using System;
using System.Threading;

namespace GzipApplication
{
    public class ChunkedReaderWriter : IDisposable
    {
        private readonly SemaphoreSlim _readSlotsSemaphore;
        private readonly AutoResetEvent _hasWriteData;
        private readonly ChunkedFileReader _chunkedFileReader;
        private readonly ChunkedDataWriter _dataWriter;
        private readonly int _bufferSize;

        public ChunkedReaderWriter(SemaphoreSlim readSlotsSemaphore, AutoResetEvent hasWriteData,
            ChunkedFileReader chunkedFileReader,
            ChunkedDataWriter dataWriter)
        {
            _readSlotsSemaphore = readSlotsSemaphore;
            _hasWriteData = hasWriteData;
            _chunkedFileReader = chunkedFileReader;
            _dataWriter = dataWriter;
            _bufferSize = readSlotsSemaphore.CurrentCount;
        }

        public void ReadWrite(Action<OrderedChunk> onRead)
        {
            bool hasMoreToRead = true;
            bool hasMoreToWrite = true;

            while (hasMoreToRead || hasMoreToWrite)
            {
                hasMoreToRead = Read(onRead, hasMoreToRead);

                hasMoreToWrite = Write(hasMoreToWrite);
            }
        }

        private bool Read(Action<OrderedChunk> onRead, bool hasMoreToRead)
        {
            int readCount = 0;

            //TODO this could lead to threads starvation
            bool BufferIsFull() => readCount >= _bufferSize;

            while (hasMoreToRead && !BufferIsFull() && _readSlotsSemaphore.Wait(TimeSpan.Zero))
            {
                OrderedChunk? chunk = _chunkedFileReader.ReadChunk();

                readCount++;

                if (!chunk.HasValue)
                {
                    return false;
                }

                onRead(chunk.Value);
            }

            return hasMoreToRead;
        }

        private bool Write(bool hasMoreToWrite)
        {
            if (hasMoreToWrite && _hasWriteData.WaitOne(TimeSpan.Zero))
            {
                hasMoreToWrite = _dataWriter.Flush();
            }

            return hasMoreToWrite;
        }

        public void Dispose()
        {
            _dataWriter.Dispose();
        }
    }
}