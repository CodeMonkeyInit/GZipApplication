using System;

namespace GzipApplication.ArgumentsParser
{
    public interface IArgumentsParser
    {
        Action ParseArguments(string[] arguments);
    }
}