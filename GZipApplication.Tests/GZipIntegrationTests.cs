using System;
using System.IO;
using GzipApplication.Compressor;
using GzipApplication.Constants;
using GZipApplication.Tests.TestData;
using Xunit;

namespace GZipApplication.Tests
{
    public class GZipIntegrationTests
    {
        private static UncompressedData DataSizeTwiceBiggerThanBuffer =>
            GetHighlyCompressibleData(ApplicationConstants.BufferSizeInBytes * 2);

        private static UncompressedData DataSizeBiggerThanBufferByOne =>
            GetHighlyCompressibleData(ApplicationConstants.BufferSizeInBytes + 1);

        private static UncompressedData DataSizeTwiceSmallerThanBuffer =>
            GetHighlyCompressibleData(ApplicationConstants.BufferSizeInBytes / 2);

        private static UncompressedData DataSizeSmallerThanBufferByOne =>
            GetHighlyCompressibleData(ApplicationConstants.BufferSizeInBytes - 1);

        private static UncompressedData DataSizeTwoTimesBiggerThanProcessorsCount =>
            GetHighlyCompressibleData(ApplicationConstants.BufferSizeInBytes * Environment.ProcessorCount * 2);

        private static UncompressedData DataSizeFourTimesBiggerThanProcessorsCount =>
            GetHighlyCompressibleData(ApplicationConstants.BufferSizeInBytes * Environment.ProcessorCount * 4);

        private static UncompressedData DataSizeEqualsOneByte =>
            GetHighlyCompressibleData(1);

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
            var gZipCompressor = new GZipCompressor();

            var outputStream = new MemoryStream();

            gZipCompressor.Execute(new MemoryStream(dataToCompress), outputStream);

            var compressedResult = outputStream.ToArray();

            return compressedResult;
        }

        private static byte[] GetDecompressData(byte[] compressedData)
        {
            var gZipDecompressor = new GZipDecompressor();

            var decompressedStream = new MemoryStream();

            gZipDecompressor.Execute(new MemoryStream(compressedData), decompressedStream);

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


        private static UncompressedData GetHighlyCompressibleData(int size)
        {
            var random = new Random();
            var bytes = new byte[size];

            random.NextBytes(bytes);

            return new UncompressedData(bytes);
        }
    }
}