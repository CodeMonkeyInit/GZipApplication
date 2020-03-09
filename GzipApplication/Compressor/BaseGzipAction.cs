using System;
using System.IO;
using System.Threading;
using GzipApplication.ChunkedReader;
using GzipApplication.ChunkedWriter;
using GzipApplication.Constants;
using GzipApplication.Data;
using GzipApplication.Exceptions.User;
using GzipApplication.WorkQueue;

namespace GzipApplication.Compressor
{
    public abstract class BaseGzipAction
    {
        public void Execute(string inputFilename, string outputFilename)
        {
            using var inputFile = GetInputFile(inputFilename);
            using var outputFile = File.Create(outputFilename);

            Execute(inputFile, outputFile);
        }

        public void Execute(Stream input, Stream output)
        {
            using var chunkedFileReader = GetReader(input);

            var writeCompletedEvent = new ManualResetEvent(false);

            // ReSharper disable once AccessToDisposedClosure
            using var fileWriter = GetFileWriter(output, () => chunkedFileReader.LengthInChunks, writeCompletedEvent);

            Execute(chunkedFileReader, fileWriter, writeCompletedEvent);
        }

        private void Execute(IChunkedReader reader, IChunkedWriter writer,
            ManualResetEvent writeCompletedEvent)
        {
            using var readSlotsSemaphore = new SemaphoreSlim(ApplicationConstants.BufferSlots);

            var evaluationStack = new IOBoundQueue();

            void ProcessingFunction(OrderedChunk chunk)
            {
                // ReSharper disable once AccessToDisposedClosure
                CpuBoundWorkQueue.Instance.QueueWork(() =>
                    ProcessChunk(chunk, readSlotsSemaphore, evaluationStack, writer));
            }

            var bufferedReader =
                new BufferedReader(reader, evaluationStack, ProcessingFunction, readSlotsSemaphore);

            var readingFunction = new Function(nameof(BufferedReader.ReadChunks),
                () => bufferedReader.ReadChunks());

            evaluationStack.Enqueue(readingFunction);

            evaluationStack.Evaluate(writeCompletedEvent);
        }

        private FileStream GetInputFile(string inputFilePath)
        {
            var fileInfo = new FileInfo(inputFilePath);

            if (!fileInfo.Exists)
                throw new FileNotFoundException(UserMessages.FileIsNotFound, inputFilePath);


            if (fileInfo.Length == 0)
                throw new InvalidFilePath(UserMessages.FileIsEmpty);

            return fileInfo.OpenRead();
        }

        protected abstract BaseChunkedReader GetReader(Stream fileStream);

        protected abstract BaseChunkedWriter GetFileWriter(Stream outputStream, Func<long?> getChunksCount,
            ManualResetEvent writeCompletedEvent);

        private void ProcessChunk(OrderedChunk chunk, SemaphoreSlim readSlotsSemaphore,
            IOBoundQueue ioBoundQueue,
            IChunkedWriter chunkedWriter)
        {
            var processedData = GetProcessedData(chunk);

            chunk.RentedData.Dispose();

            var orderedChunk = new OrderedChunk
            {
                RentedData = processedData,
                Order = chunk.Order
            };

            var writingFunction = new Function(nameof(IChunkedWriter.WriteOrAddChunk),
                () => chunkedWriter.WriteOrAddChunk(orderedChunk));

            ioBoundQueue.Enqueue(writingFunction);

            readSlotsSemaphore.Release();
        }

        public static int CalculateBufferOverhead(int bufferSize)
        {
            const int gzipOverheadOver32KbInBytes = 5;

            var bufferOverhead = (int) Math.Ceiling((double) bufferSize / 32) * gzipOverheadOver32KbInBytes;

            return bufferOverhead;
        }

        public static int CalculateArchiveMaxSize(int bufferSize)
        {
            return bufferSize + ApplicationConstants.GzipStreamHeaderAndFooterInBytes +
                   CalculateBufferOverhead(bufferSize);
        }


        protected abstract RentedArray<byte> GetProcessedData(OrderedChunk chunk);
    }
}