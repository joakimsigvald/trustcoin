using System;

namespace Trustcoin.Story
{
    public interface ILogger
    {
        void Log(string message, ConsoleColor color = ConsoleColor.Cyan);
    }
}