using System;

namespace GzipApplication
{
    public struct OrderedChunk
    {
        public long Order;
        public Memory<byte> Data;
    }
}