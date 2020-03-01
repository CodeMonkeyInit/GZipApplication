using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace GzipApplication
{
    public class GZipCompressor
    {
        public void Compress(string inputFilename, string outputFilename) =>
            Process(inputFilename, outputFilename, CompressionMode.Compress);

        public void Decompress(string inputFilename, string outputFilename) =>
            Process(inputFilename, outputFilename, CompressionMode.Decompress);

        private void Process(string inputFileInfo, string outputFilename, CompressionMode compressionMode)
        {
            ValidateFile(inputFileInfo);

            const int bufferMultiplier = 2;

            using var readSlotsSemaphore = new SemaphoreSlim(CpuBoundWorkQueue.ParallelWorkMax * bufferMultiplier);
            using var hasWriteDataEvent = new AutoResetEvent(false);

            var processedChunks = new ConcurrentBag<OrderedChunk>();

            using var fileReader = new ChunkedFileReader(File.OpenRead(inputFileInfo));

            var processingFunction = GetDataProcessingFunction(processedChunks, hasWriteDataEvent, 
                compressionMode, readSlotsSemaphore);

            var chunksCount = fileReader.LengthInChunks;

            using var compressedDataWriter =
                new ChunkedDataWriter(outputFilename, processedChunks, chunksCount);

            using var readerWriter =
                new ChunkedReaderWriter(readSlotsSemaphore, hasWriteDataEvent, fileReader, compressedDataWriter);

            readerWriter.ReadWrite(processingFunction);
        }

        private Action<OrderedChunk> GetDataProcessingFunction(ConcurrentBag<OrderedChunk> compressedChunks,
            EventWaitHandle dataCompressedEvent, CompressionMode compressionMode, SemaphoreSlim readSlotsSemaphore)
        {
            return chunk =>
                CpuBoundWorkQueue.QueueWork(() =>
                    ProcessChunk(chunk, compressedChunks, dataCompressedEvent, compressionMode, readSlotsSemaphore));
        }

        private static void ProcessChunk(OrderedChunk chunkToProcess,
            ConcurrentBag<OrderedChunk> processedChunks, EventWaitHandle chunkProcessedEventHandle,
            CompressionMode compressionMode, SemaphoreSlim readSlotsSemaphore)
        {
            using var memoryStream = new MemoryStream();
            using var gZipStream = new GZipStream(memoryStream, compressionMode);

            gZipStream.Write(chunkToProcess.Data);

            processedChunks.Add(new OrderedChunk
            {
                Data = memoryStream.ToArray(),
                Order = chunkToProcess.Order
            });

            chunkProcessedEventHandle.Set();
            readSlotsSemaphore.Release();
        }

        private void ValidateFile(string inputFileInfo)
        {
            if (!File.Exists(inputFileInfo))
            {
                throw new FileNotFoundException("Sorry the file you specified is not found", inputFileInfo);
            }
        }
    }
}