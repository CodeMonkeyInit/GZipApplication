using System;
using System.Buffers;
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
    /// <summary>
    ///     Base class for Gzip actions.
    /// </summary>
    public abstract class BaseGzipAction
    {
        public const long ArchiveHeader = 0xBAD_DEAD_1337_C0DE;

        public void Execute(string inputFilename, string outputFilename)
        {
            using var inputFile = GetInputFile(inputFilename);
            using var outputFile = File.Create(outputFilename);

            Execute(inputFile, outputFile);
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

        public virtual void Execute(Stream input, Stream output)
        {
            using var chunkedFileReader = GetReader(input);

            var writeCompletedEvent = new ManualResetEvent(false);

            // ReSharper disable once AccessToDisposedClosure
            using var fileWriter = GetWriter(output, () => chunkedFileReader.LengthInChunks, writeCompletedEvent);

            Execute(chunkedFileReader, fileWriter, writeCompletedEvent);
        }

        private void Execute(IChunkedReader reader, IChunkedWriter writer,
            ManualResetEvent writeCompletedEvent)
        {
            using var readSlotsSemaphore = new SemaphoreSlim(ApplicationConstants.BufferSlots);

            var evaluationStack = new IOBoundQueue();

            void ProcessingFunction(OrderedChunk chunk) =>
                // ReSharper disable once AccessToDisposedClosure
                CpuBoundWorkQueue.Instance.QueueWork(() =>
                    ProcessChunk(chunk, readSlotsSemaphore, evaluationStack, writer));

            var bufferedReader =
                new BufferedReader(reader, evaluationStack, ProcessingFunction, readSlotsSemaphore);

            var readingFunction = new Function(nameof(BufferedReader.ReadChunks),
                () => bufferedReader.ReadChunks());

            evaluationStack.Enqueue(readingFunction);

            evaluationStack.Evaluate(writeCompletedEvent);
        }

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

            var writingFunction = new Function(nameof(IChunkedWriter.WriteOrStoreChunk),
                () => chunkedWriter.WriteOrStoreChunk(orderedChunk));

            ioBoundQueue.Enqueue(writingFunction);

            readSlotsSemaphore.Release();
        }

        /// <summary>
        ///     Calculates archive max size including GZip overhead and header and footer.
        /// </summary>
        /// <returns>Archive max size in bytes.</returns>
        protected static int CalculateArchiveMaxSizeInBytes(int bufferSizeInBytes) =>
            bufferSizeInBytes + ApplicationConstants.GzipStreamHeaderAndFooterInBytes +
            CalculateBufferOverheadInBytes(bufferSizeInBytes);

        private static int CalculateBufferOverheadInBytes(int bufferSizeInBytes)
        {
            const int gzipOverheadOver32KbInBytes = 5;

            var bufferOverhead = (int) Math.Ceiling((double) bufferSizeInBytes / 32) * gzipOverheadOver32KbInBytes;

            return bufferOverhead;
        }

        /// <summary>
        ///     Processes <see cref="OrderedChunk"/> data.
        /// </summary>
        /// <returns>Array rented from <see cref="ArrayPool{T}"/></returns>
        protected abstract RentedArray<byte> GetProcessedData(OrderedChunk chunk);

        protected abstract BaseChunkedReader GetReader(Stream inputStream);

        protected abstract BaseChunkedWriter GetWriter(Stream outputStream, Func<long?> getChunksCount,
            ManualResetEvent writeCompletedEvent);
    }
}