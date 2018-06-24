using System;

namespace Trustcoin.Story
{
    public interface Logger
    {
        void Log(string message, ConsoleColor color = ConsoleColor.Cyan);
    }
}