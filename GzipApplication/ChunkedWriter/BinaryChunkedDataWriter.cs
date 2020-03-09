using System;
using System.IO;
using System.Threading;
using GzipApplication.Data;

namespace GzipApplication.ChunkedWriter
{
    public class BinaryChunkedDataWriter : BaseChunkedWriter
    {
        private readonly BinaryWriter _binaryWriter;

        public BinaryChunkedDataWriter(Stream output, Func<long?> getChunksCount, ManualResetEvent writeCompletedEvent)
            : base(getChunksCount, writeCompletedEvent)
        {
            _binaryWriter = new BinaryWriter(output);
        }

        protected override void Write(OrderedChunk chunk)
        {
            _binaryWriter.Write(chunk.RentedData.RentedLength);
            _binaryWriter.Write(chunk.RentedData.Array, 0, chunk.RentedData.RentedLength);
        }

        protected override void Flush()
        {
            _binaryWriter.Flush();
        }

        public override void Dispose()
        {
            _binaryWriter.Dispose();
        }
    }
}