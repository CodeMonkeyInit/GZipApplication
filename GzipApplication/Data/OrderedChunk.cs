using System;

namespace GzipApplication.Data
{
    public struct OrderedChunk
    {
        public long Order;
        public Memory<byte> Data;
    }
}