using System;
using System.Threading;
using GzipApplication.ChunkedFileReader;
using GzipApplication.ChunkedFIleWriter;

namespace GzipApplication
{
    public class ChunkedReaderWriter : IDisposable
    {
        private readonly SemaphoreSlim _readSlotsSemaphore;
        private readonly AutoResetEvent _hasWriteData;
        private readonly IChunkedFileReader _fixLengthChunkedFileReader;
        private readonly BaseChunkedWriter _dataWriter;
        private readonly int _bufferSize;

        public ChunkedReaderWriter(SemaphoreSlim readSlotsSemaphore, AutoResetEvent hasWriteData,
            IChunkedFileReader fixLengthChunkedFileReader, BaseChunkedWriter dataWriter)
        {
            _readSlotsSemaphore = readSlotsSemaphore;
            _hasWriteData = hasWriteData;
            _fixLengthChunkedFileReader = fixLengthChunkedFileReader;
            _dataWriter = dataWriter;
            _bufferSize = readSlotsSemaphore.CurrentCount;
        }

        public void ReadWrite(Action<OrderedChunk> onRead)
        {
            bool hasMoreToRead = true;
            bool hasMoreToWrite = true;

            while (hasMoreToRead || hasMoreToWrite)
            {
                hasMoreToRead = Read(onRead);

                hasMoreToWrite = Write(hasMoreToWrite);
            }
        }

        private bool Read(Action<OrderedChunk> onRead)
        {
            int readCount = 0;

            //TODO this could lead to threads starvation
            bool BufferIsFull() => readCount >= _bufferSize;

            bool hasMoreToRead = _fixLengthChunkedFileReader.HasMore;
            while (hasMoreToRead && !BufferIsFull() && _readSlotsSemaphore.Wait(TimeSpan.Zero))
            {
                OrderedChunk chunk = _fixLengthChunkedFileReader.ReadChunk();

                readCount++;

                onRead(chunk);

                hasMoreToRead = _fixLengthChunkedFileReader.HasMore;
            }

            return hasMoreToRead;
        }

        private bool Write(bool hasMoreToWrite)
        {
            if (hasMoreToWrite && _hasWriteData.WaitOne(TimeSpan.Zero))
            {
                hasMoreToWrite = _dataWriter.FlushReadyChunks();
            }

            return hasMoreToWrite;
        }

        public void Dispose()
        {
            _dataWriter.Dispose();
        }
    }
}