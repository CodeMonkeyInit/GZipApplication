using System;
using System.IO;
using System.Threading;
using GzipApplication.Data;

namespace GzipApplication.ChunkedWriter
{
    /// <summary>
    /// <inheritdoc/>
    ///     Writes chunk length and contents using <see cref="BinaryWriter"/>
    /// </summary>
    public class BinaryChunkedDataWriter : BaseChunkedWriter
    {
        private readonly BinaryWriter _binaryWriter;

        /// <inheritdoc />
        public BinaryChunkedDataWriter(Stream outputToWrite, Func<long?> getChunksCount,
            ManualResetEvent writeCompletedEvent)
            : base(getChunksCount, writeCompletedEvent)
        {
            _binaryWriter = new BinaryWriter(outputToWrite);
        }

        protected override void Write(OrderedChunk chunk)
        {
            _binaryWriter.Write(chunk.RentedData.RentedLength);
            _binaryWriter.Write(chunk.RentedData.AsBoundedSpan);
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