using System;
using Trustcoin.Story;

namespace Trustcoin.Main
{
    public class NoLogger : ILogger
    {
        public void Log(string message, ConsoleColor color = ConsoleColor.Cyan)
        {
        }
    }
}
