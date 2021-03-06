using System;
using GzipApplication.Constants;
using GzipApplication.Exceptions.User;
using GzipApplication.Files;
using GzipApplication.GZip;

namespace GzipApplication.ArgumentsParser
{
    public class ApplicationArgumentsParser : IArgumentsParser
    {
        private const int CommandIndex = 0;
        private const int InputFileIndex = 1;
        private const int OutputFileIndex = 2;

        private const int RequiredArgumentsCount = 3;

        public Action ParseArguments(string[] arguments)
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

            IFileService fileService = FileServiceFactory.GetFileService();

            var delegateToCall = GetDelegateToCall(command, fileService);

            return () => delegateToCall(inputFilePath, outputFilePath);
        }

        private static void ValidatePath(string filePath, string name)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new InvalidFilePath(string.Format(UserMessages.FilepathIsInvalidFormat, name, filePath));
        }

        private static Action<string, string> GetDelegateToCall(string command, IFileService fileService) =>
            command switch
            {
                Commands.Compression => new GZipCompressor(fileService).Execute,
                Commands.Decompression => new GZipDecompressor(fileService).Execute,
                _ => throw new InvalidArgumentException(
                    $"{string.Format(UserMessages.InvalidArgumentFormat, command)} {UserMessages.ValidArgumentsFormat}")
            };
    }
}