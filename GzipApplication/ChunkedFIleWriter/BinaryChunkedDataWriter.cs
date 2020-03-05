using System;
using System.IO;
using GzipApplication.Data;

namespace GzipApplication.ChunkedFIleWriter
{
    public class BinaryChunkedDataWriter : BaseChunkedWriter
    {
        private readonly BinaryWriter _binaryWriter;

        public BinaryChunkedDataWriter(string outputFilename, Func<long?> getChunksCount) : base(getChunksCount)
        {
            _binaryWriter = new BinaryWriter(File.Create(outputFilename));
        }

        protected override void Write(OrderedChunk chunk)
        {
            _binaryWriter.Write(chunk.Data.Length);
            _binaryWriter.Write(chunk.Data.Span);
        }

        protected override void Flush() => _binaryWriter.Flush();

        public override void Dispose()
        {
            _binaryWriter.Dispose();
        }
    }
}