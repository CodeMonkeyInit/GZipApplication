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
            
            gZipCompressor.Compress("/Users/denis/Desktop/CompressTest/vk.dmg", 
                "/Users/denis/Desktop/CompressTest/vk.dmg.shiet");
            
            stopwatch.Stop();
            
            Console.WriteLine($"Done in {stopwatch.Elapsed}");
        }
    }
}