using System;
using Trustcoin.Story;

namespace Trustcoin.Main
{
    public class NoLogger : Logger
    {
        public void Log(string message, ConsoleColor color = ConsoleColor.Cyan)
        {
        }
    }
}
