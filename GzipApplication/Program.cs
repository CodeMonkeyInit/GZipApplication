using System;
using System.Diagnostics;
using System.IO;
using GzipApplication.Exceptions;

namespace GzipApplication
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var stopwatch = new Stopwatch();

            var argumentsParser = new ArgumentsParser.ArgumentsParser();
            
            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
            {
                Console.WriteLine(((Exception) eventArgs.ExceptionObject).Message);

                Environment.Exit(1);
            };

            try
            {
                var action = argumentsParser.GetAction(args);

                stopwatch.Start();

                action();

                stopwatch.Stop();

                Console.WriteLine($"Done in {stopwatch.Elapsed}");

                return 0;
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine($"{e.Message} Filename: {e.FileName}");
            }
            catch (Exception e) when (e is InvalidDataException || e is UserReadableException)
            {
                Console.WriteLine(e.Message);
            }

            return -1;
        }
    }
}