using System;
using System.IO;
using GzipApplication.Constants;
using GzipApplication.GZip;
using GZipApplication.Tests.TestData;
using Xunit;

namespace GZipApplication.Tests
{
    public class GZipIntegrationTests
    {
        [MemberData(nameof(GetBufferTestData))]
        [Theory]
        public void OriginalDataMatchesDecompressed(UncompressedData dataToCompress)
        {
            var compressedData = GetCompressedData(dataToCompress.Data);

            var decompressedBytes = GetDecompressData(compressedData);

            Assert.Equal(dataToCompress.Data, decompressedBytes);
        }

        private static byte[] GetCompressedData(byte[] dataToCompress)
        {
            var gZipCompressor = new GZipCompressor(new InMemoryFileService());

            var outputStream = new MemoryStream();

            gZipCompressor.Execute(new MemoryStream(dataToCompress), outputStream, true);

            var compressedResult = outputStream.ToArray();

            return compressedResult;
        }

        private static byte[] GetDecompressData(byte[] compressedData)
        {
            var gZipDecompressor = new GZipDecompressor(new InMemoryFileService());

            var decompressedStream = new MemoryStream();

            gZipDecompressor.Execute(new MemoryStream(compressedData), decompressedStream, true);

            var decompressedBytes = decompressedStream.ToArray();

            return decompressedBytes;
        }

        public static TheoryData<UncompressedData> GetBufferTestData()
        {
            return new TheoryData<UncompressedData>
            {
                DataSizeTwiceBiggerThanBuffer,
                DataSizeBiggerThanBufferByOne,
                DataSizeTwiceSmallerThanBuffer,
                DataSizeSmallerThanBufferByOne,
                DataSizeTwoTimesBiggerThanProcessorsCount,
                DataSizeFourTimesBiggerThanProcessorsCount,
                DataSizeEqualsOneByte
            };
        }

        private static UncompressedData DataSizeTwiceBiggerThanBuffer =>
            GetRandomData(ApplicationConstants.BufferSizeInBytes * 2);

        private static UncompressedData DataSizeBiggerThanBufferByOne =>
            GetRandomData(ApplicationConstants.BufferSizeInBytes + 1);

        private static UncompressedData DataSizeTwiceSmallerThanBuffer =>
            GetRandomData(ApplicationConstants.BufferSizeInBytes / 2);

        private static UncompressedData DataSizeSmallerThanBufferByOne =>
            GetRandomData(ApplicationConstants.BufferSizeInBytes - 1);

        private static UncompressedData DataSizeTwoTimesBiggerThanProcessorsCount =>
            GetRandomData(ApplicationConstants.BufferSizeInBytes * Environment.ProcessorCount * 2);

        private static UncompressedData DataSizeFourTimesBiggerThanProcessorsCount =>
            GetRandomData(ApplicationConstants.BufferSizeInBytes * Environment.ProcessorCount * 4);

        private static UncompressedData DataSizeEqualsOneByte =>
            GetRandomData(1);

        private static UncompressedData GetRandomData(int size)
        {
            var random = new Random();
            var bytes = new byte[size];

            random.NextBytes(bytes);

            return new UncompressedData(bytes);
        }
    }
}