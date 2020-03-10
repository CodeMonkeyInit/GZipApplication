using System;
using System.IO;
using GzipApplication.ArgumentsParser;
using GzipApplication.Exceptions.User;

namespace GzipApplication
{
    public class Program
    {
        private const int ProgramFailedExitCode = 1;
        private const int ProgramSucceededExitCode = 0;

        public static int Main(string[] args)
        {
            var argumentsParser = new ApplicationArgumentsParser();

            SubscribeToThreadExceptions();

            try
            {
                var action = argumentsParser.ParseArguments(args);

                Console.WriteLine("Execution started. Please wait...");

                action();

                Console.WriteLine("Execution complete");

                return ProgramSucceededExitCode;
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine($"{e.Message} Filename: {e.FileName}");
            }
            catch (Exception e) when (e is InvalidDataException || e is UserReadableException)
            {
                Console.WriteLine(e.Message);
            }

            return ProgramFailedExitCode;
        }

        private static void SubscribeToThreadExceptions()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
            {
                Console.WriteLine(((Exception) eventArgs.ExceptionObject).Message);

                Environment.Exit(ProgramFailedExitCode);
            };
        }
    }
}