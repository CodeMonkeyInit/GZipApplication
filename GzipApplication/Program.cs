using System;
using System.Diagnostics;

namespace GzipApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var stopwatch = new Stopwatch();
            
            var gZipCompressor = new GZipCompressor();
            
            stopwatch.Start();
            
            gZipCompressor.Compress("/Users/deniskuliev/Downloads/Reflector 2.5.4/Reflector 2 v2.5.4.dmg",
                "/Users/deniskuliev/Downloads/Reflector 2.5.4/Reflector 2 v2.5.4.dmg.gz");
            
            gZipCompressor.Decompress("/Users/deniskuliev/Downloads/Reflector 2.5.4/Reflector 2 v2.5.4.dmg.gz", 
                "/Users/deniskuliev/Downloads/Reflector 2.5.4/Reflector 2 v2.5.4.dmg.gz.dmg");
            
            stopwatch.Stop();
            
            Console.WriteLine($"Done in {stopwatch.Elapsed}");
        }
    }
}