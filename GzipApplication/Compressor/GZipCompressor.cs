using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Compression;
using System.Threading;
using GzipApplication.ChunkedFileReader;
using GzipApplication.ChunkedFIleWriter;
using GzipApplication.Constants;
using GzipApplication.Data;
using GzipApplication.Exceptions;
using GzipApplication.Exceptions.User;
using GzipApplication.ReaderWriter;
using GzipApplication.WorkQueue;

namespace GzipApplication.Compressor
{
    public class GZipCompressor : IGZipCompressor
    {
        public void Compress(string inputFilename, string outputFilename)
        {
            ValidateFile(inputFilename);

            var processedChunks = new ConcurrentBag<OrderedChunk>();

            using var fileReader = new FixLengthChunkedFileReader(File.OpenRead(inputFilename));
            using var fileWriter = new BinaryChunkedDataWriter(outputFilename, processedChunks, 
                // ReSharper disable once AccessToDisposedClosure
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
            
            using var compressedDataStream = new MemoryStream(chunk.Data.ToArray());
            using var gZipStream = new GZipStream(compressedDataStream, CompressionMode.Decompress);

            try
            {
                gZipStream.CopyTo(memoryStream);
                gZipStream.Flush();
            }
            catch (InvalidDataException e)
            {
                throw new InvalidArchiveFormatException(UserMessages.ArchiveFormatIsNotSupported, e);
            }
            
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
            
            gZipStream.Write(chunkToProcess.Data.Span);
            gZipStream.Flush();
            
            processedChunks.Add(new OrderedChunk
            {
                Data = memoryStream.ToArray(),
                Order = chunkToProcess.Order
            });

            chunkProcessedEventHandle.Set(); 
            readSlotsSemaphore.Release();
        }
        
        private void ValidateFile(string inputFilePath)
        {
            if (!File.Exists(inputFilePath))
            {
                throw new FileNotFoundException(UserMessages.FileIsNotFound, inputFilePath);
            }
        }
    }
}