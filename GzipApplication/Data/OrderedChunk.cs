namespace GzipApplication.Data
{
    public struct OrderedChunk
    {
        public long Order;
        public RentedArray<byte> RentedData;
    }
}