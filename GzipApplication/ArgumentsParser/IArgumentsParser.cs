using System;

namespace GzipApplication.ArgumentsParser
{
    public interface IArgumentsParser
    {
        Action GetAction(string[] arguments);
    }
}