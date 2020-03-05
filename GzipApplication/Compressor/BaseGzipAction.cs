using System;
using System.IO;
using System.Threading;
using GzipApplication.ChunkedFileReader;
using GzipApplication.ChunkedFIleWriter;
using GzipApplication.Constants;
using GzipApplication.Data;
using GzipApplication.ReaderWriter;
using GzipApplication.WorkQueue;

namespace GzipApplication.Compressor
{
    public abstract class BaseGzipAction
    {
        public void Execute(string inputFilename, string outputFilename)
        {
            ValidateFile(inputFilename);

            using var chunkedFileReader = GetFileReader(File.OpenRead(inputFilename));
            // ReSharper disable once AccessToDisposedClosure
            using var fileWriter = GetFileWriter(outputFilename, () => chunkedFileReader.LengthInChunks);

            Process(chunkedFileReader, fileWriter);
        }
        
        private void ValidateFile(string inputFilePath)
        {
            if (!File.Exists(inputFilePath))
            {
                throw new FileNotFoundException(UserMessages.FileIsNotFound, inputFilePath);
            }
        }

        protected abstract BaseChunkedFileReader GetFileReader(FileStream fileStream);

        protected abstract BaseChunkedWriter GetFileWriter(string filename, Func<long?> getChunksCount);
        
        private void Process(IChunkedFileReader fileReader, BaseChunkedWriter writer)
        {
            const int bufferMultiplier = 2;

            using var readSlotsSemaphore = new SemaphoreSlim(CpuBoundWorkQueue.ParallelWorkMax * bufferMultiplier);

            var evaluationStack = new EvaluationQueue();

            void ProcessingFunction(OrderedChunk chunk) => 
                // ReSharper disable once AccessToDisposedClosure
                CpuBoundWorkQueue.QueueWork(() => ProcessChunk(chunk, readSlotsSemaphore, evaluationStack, writer));

            var bufferedReader =
                new BufferedReader(fileReader, evaluationStack, ProcessingFunction, readSlotsSemaphore);

            var readingFunction = new Function(nameof(BufferedReader.ReadChunks), 
                () => bufferedReader.ReadChunks());
            
            evaluationStack.Enqueue(readingFunction);

            evaluationStack.Evaluate();
        }

        private void ProcessChunk(OrderedChunk chunk, SemaphoreSlim readSlotsSemaphore,
            EvaluationQueue evaluationQueue,
            IChunkedDataWriter chunkedWriter)
        {
            var memoryStream = GetProcessedMemoryStream(chunk);

            var orderedChunk = new OrderedChunk
            {
                Data = memoryStream.ToArray(),
                Order = chunk.Order
            };

            var writingFunction = new Function(nameof(IChunkedDataWriter.WriteOrAddChunk), 
                () => chunkedWriter.WriteOrAddChunk(orderedChunk));
            
            evaluationQueue.Enqueue(writingFunction);

            readSlotsSemaphore.Release();
        }

        protected abstract MemoryStream GetProcessedMemoryStream(OrderedChunk chunk);
    }
}