using System;
using GzipApplication.Compressor;
using GzipApplication.Constants;
using GzipApplication.Exceptions;
using GzipApplication.Exceptions.User;

namespace GzipApplication.ArgumentsParser
{
    public class ArgumentsParser : IArgumentsParser
    {
        private const int CommandIndex = 0;
        private const int InputFileIndex = 1;
        private const int OutputFileIndex = 2;

        private const int RequiredArgumentsCount = 3;

        public Action GetAction(string[] arguments)
        {
            if (arguments.Length == 0)
                throw new EmptyArgumentsException($"{UserMessages.EmptyArguments} {UserMessages.SpecifyArguments}");

            if (arguments.Length < RequiredArgumentsCount)
                throw new InvalidArgumentException(
                    $"{UserMessages.SomeArgumentsAreMissing} {UserMessages.SpecifyArguments}");

            var command = arguments[CommandIndex];
            var inputFilePath = arguments[InputFileIndex];
            var outputFilePath = arguments[OutputFileIndex];

            ValidatePath(inputFilePath, "Input");
            ValidatePath(outputFilePath, "Output");
            
            var delegateToCall = GetDelegateToCall(command);

            return () => delegateToCall(inputFilePath, outputFilePath);
        }

        private static void ValidatePath(string filePath, string name)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new InvalidFilePath(string.Format(UserMessages.FilepathIsInvalidFormat, name, filePath));
        }

        private static Action<string, string> GetDelegateToCall(string command)
        {
            var gZipCompressor = new GZipCompressor();

            Action<string, string> delegateToCall = command switch
            {
                Commands.Compression => new GZipCompressor().Execute,
                Commands.Decompression => new GZipDecompressor().Execute,
                _ => throw new InvalidArgumentException(
                    $"{string.Format(UserMessages.InvalidArgumentFormat, command)} {UserMessages.ValidArgumentsFormat}")
            };

            return delegateToCall;
        }
    }
}