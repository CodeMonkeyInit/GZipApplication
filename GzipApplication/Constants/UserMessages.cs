namespace GzipApplication.Constants
{
    /// <summary>
    ///     Messages intended for user to display errors in readable way.
    /// </summary>
    public static class UserMessages
    {
        public const string EmptyArguments = "No arguments specified.";
        public const string SomeArgumentsAreMissing = "Some arguments are missing.";
        public const string SpecifyArguments = "Please specify arguments.";

        public const string FileIsNotFound = "Sorry the file you specified is not found.";

        public const string InvalidArgumentFormat = "Sorry command '{0}' is invalid.";

        public static string ValidArgumentsFormat => $"Valid commands: {Commands.Compression}/{Commands.Decompression}";
        public static string FileIsEmpty = "Sorry, the file you provided is empty, so there is nothing to compress";

        public const string FilepathIsInvalidFormat = "{0} filepath: '{1}' is invalid.";
        public const string UnableToCreateOutputFileFormat = "Cannot create output file. Reason: {0}";

        public const string UnreadableFileFormat =
            "Can't read file {0}. Looks like file is in use by another program. Or Program doesn't have enough permissions";

        public const string ArchiveFormatIsNotSupported = "Decompression of this archive format is not supported.";
    }
}