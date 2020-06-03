using System;
using System.Buffers;
using System.IO;
using System.Threading;
using GzipApplication.ChunkedReader;
using GzipApplication.ChunkedReader.BufferedReader;
using GzipApplication.ChunkedWriter;
using GzipApplication.Constants;
using GzipApplication.Data;
using GzipApplication.Exceptions.User;
using GzipApplication.Files;
using GzipApplication.WorkQueue;

namespace GzipApplication.GZip
{
    /// <summary>
    ///     Base class for Gzip actions.
    /// </summary>
    public abstract class BaseGzipAction : IGzipProcessor
    {
        public const long ArchiveHeader = 0xBAD_DEAD_1337_C0DE;

        private readonly IFileService _fileService;

        protected BaseGzipAction(IFileService fileService)
        {
            _fileService = fileService;
        }

        public void Execute(string inputFilename, string outputFilename)
        {
            using var inputFile = GetInputFile(inputFilename);
            using var outputFile = GetOutputFile(outputFilename);

            bool ioOnDifferentDrives = _fileService.IsFilesOnDifferentDrives(inputFilename, outputFilename);

            Execute(inputFile, outputFile, ioOnDifferentDrives);
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

        private FileStream GetOutputFile(string outputFilename)
        {
            try
            {
                return File.Create(outputFilename);
            }
            catch (Exception e) when (e is UnauthorizedAccessException
                                      || e is PathTooLongException
                                      || e is DirectoryNotFoundException
                                      || e is IOException)
            {
                throw new InvalidFilePath(string.Format(UserMessages.UnableToCreateOutputFileFormat, e.Message));
            }
        }

        public virtual void Execute(Stream input, Stream output, bool ioIsOnDifferentDrives)
        {
            using var chunkedFileReader = GetReader(input);

            var writeCompletedEvent = new ManualResetEvent(false);

            // ReSharper disable once AccessToDisposedClosure
            using var fileWriter = GetWriter(output, () => chunkedFileReader.LengthInChunks, writeCompletedEvent);

            Execute(chunkedFileReader, fileWriter, writeCompletedEvent, ioIsOnDifferentDrives);
        }

        private void Execute(IChunkedReader reader, IChunkedWriter writer,
            ManualResetEvent writeCompletedEvent, bool ioIsOnDifferentDrives)
        {
            using var readSlotsSemaphore = new SemaphoreSlim(ApplicationConstants.BufferSlots);

            var writerQueue = new IOBoundQueue();
            var readerQueue = ioIsOnDifferentDrives ? new IOBoundQueue() : writerQueue;

            var chunkProcessor = new GzipChunkProcessor(readSlotsSemaphore, writerQueue, writer, this);

            void ProcessingFunction(OrderedChunk chunk) =>
                CpuBoundWorkQueue.Instance.QueueWork(() => chunkProcessor.Process(chunk));

            var bufferedReader =
                new BufferedReader(reader, readerQueue, ProcessingFunction, readSlotsSemaphore);

            var readingFunction = new Function(nameof(BufferedReader.ReadChunks),
                () => bufferedReader.ReadChunks());

            readerQueue.Enqueue(readingFunction);

            if (ioIsOnDifferentDrives)
                new Thread(() => readerQueue.Evaluate()) {IsBackground = true}.Start();

            writerQueue.Evaluate(writeCompletedEvent);
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