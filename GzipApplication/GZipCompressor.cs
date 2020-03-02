using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Compression;
using System.Threading;
using GzipApplication.ChunkedFileReader;
using GzipApplication.ChunkedFIleWriter;

namespace GzipApplication
{
    public class GZipCompressor
    {
        public void Compress(string inputFilename, string outputFilename)
        {
            ValidateFile(inputFilename);

            var processedChunks = new ConcurrentBag<OrderedChunk>();

            using var fileReader = new FixLengthChunkedFileReader(File.OpenRead(inputFilename));
            using var fileWriter = new BinaryChunkedDataWriter(outputFilename, processedChunks, 
                () => fileReader.LengthInChunks);
            
            Process(CompressionMode.Compress, fileReader, processedChunks, fileWriter);
        }

        public void Decompress(string inputFilename, string outputFilename)
        {
            ValidateFile(inputFilename);
            
            var processedChunks = new ConcurrentBag<OrderedChunk>();

            using var fileReader = new BinaryChunkedFileReader(File.OpenRead(inputFilename));
            using var fileWriter = new ChunkWriter(outputFilename, processedChunks, () => fileReader.LengthInChunks);
            
            Process(CompressionMode.Decompress, fileReader, processedChunks, fileWriter);
        }

        private void Process(CompressionMode compressionMode,
            IChunkedFileReader fileReader, ConcurrentBag<OrderedChunk> processedChunks, BaseChunkedWriter writer)
        {
            const int bufferMultiplier = 2;

            using var readSlotsSemaphore = new SemaphoreSlim(CpuBoundWorkQueue.ParallelWorkMax * bufferMultiplier);
            using var hasWriteDataEvent = new AutoResetEvent(false);

            var processingFunction = GetDataProcessingFunction(processedChunks, hasWriteDataEvent, 
                compressionMode, readSlotsSemaphore);

            using var readerWriter =
                new ChunkedReaderWriter(readSlotsSemaphore, hasWriteDataEvent, fileReader, writer);

            readerWriter.ReadWrite(processingFunction);
        }

        private Action<OrderedChunk> GetDataProcessingFunction(ConcurrentBag<OrderedChunk> processedChunks,
            EventWaitHandle dataCompressedEvent, CompressionMode compressionMode, SemaphoreSlim readSlotsSemaphore)
        {
            if (compressionMode == CompressionMode.Compress)
            {
                return chunk =>
                    CpuBoundWorkQueue.QueueWork(() =>
                        CompressChunk(chunk, processedChunks, dataCompressedEvent, readSlotsSemaphore));
            }
            
            return chunk =>
                CpuBoundWorkQueue.QueueWork(() =>
                    DecompressChunk(chunk, processedChunks, dataCompressedEvent, readSlotsSemaphore));
           
        }

        private static void DecompressChunk(OrderedChunk chunk, ConcurrentBag<OrderedChunk> processedChunks,
            EventWaitHandle chunkProcessedEventHandle,
            SemaphoreSlim readSlotsSemaphore)
        {
            using var memoryStream = new MemoryStream();
            using var gZipStream = new GZipStream(new MemoryStream(chunk.Data), CompressionMode.Decompress);

            gZipStream.CopyTo(memoryStream);
            gZipStream.Flush();

            processedChunks.Add(new OrderedChunk
            {
                Data = memoryStream.ToArray(),
                Order = chunk.Order
            });

            chunkProcessedEventHandle.Set(); 
            readSlotsSemaphore.Release();
        }

        private static void CompressChunk(OrderedChunk chunkToProcess,
            ConcurrentBag<OrderedChunk> processedChunks, EventWaitHandle chunkProcessedEventHandle, SemaphoreSlim readSlotsSemaphore)
        {
            using var memoryStream = new MemoryStream();
            using var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress);

            gZipStream.Write(chunkToProcess.Data);
            gZipStream.Flush();

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