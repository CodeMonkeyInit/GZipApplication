using System;
using System.IO;
using System.Threading;
using GzipApplication.ChunkedReader;
using GzipApplication.ChunkedReader.BufferedReader;
using GzipApplication.ChunkedWriter;
using GzipApplication.Constants;
using GzipApplication.Data;
using GzipApplication.Exceptions.User;
using GzipApplication.WorkQueue;

namespace GzipApplication.GZip
{
    /// <summary>
    ///     Base class for Gzip actions.
    /// </summary>
    public abstract partial class BaseGzipAction : IGzipProcessor
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

            var ioBoundQueue = new IOBoundQueue();

            var chunkProcessor = new GzipChunkProcessor(readSlotsSemaphore, ioBoundQueue, writer, this);

            void ProcessingFunction(OrderedChunk chunk) =>
                CpuBoundWorkQueue.Instance.QueueWork(() => chunkProcessor.Process(chunk));

            var bufferedReader =
                new BufferedReader(reader, ioBoundQueue, ProcessingFunction, readSlotsSemaphore);

            var readingFunction = new Function(nameof(BufferedReader.ReadChunks),
                () => bufferedReader.ReadChunks());

            ioBoundQueue.Enqueue(readingFunction);

            ioBoundQueue.Evaluate(writeCompletedEvent);
        }

        protected abstract BaseChunkedReader GetReader(Stream inputStream);

        protected abstract BaseChunkedWriter GetWriter(Stream outputStream, Func<long?> getChunksCount,
            ManualResetEvent writeCompletedEvent);

        /// <summary>
        ///     Processes <see cref="OrderedChunk"/> data.
        /// </summary>
        /// <returns>Array rented from <see cref="ArrayPool{T}"/></returns>
        public abstract RentedArray<byte> GetProcessedData(OrderedChunk chunk);
    }
}