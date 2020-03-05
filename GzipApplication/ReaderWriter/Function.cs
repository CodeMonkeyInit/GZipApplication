using System;

namespace GzipApplication.ReaderWriter
{
    public struct Function
    {
        public string Name;

        public Func<bool> Payload;

        public Function(string name, Func<bool> payload)
        {
            Name = name;
            Payload = payload;
        }
    }
}