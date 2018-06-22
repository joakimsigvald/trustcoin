using System;
using Trustcoin.Story;

namespace Trustcoin.Main
{
    public class StopLogger : ILogger
    {
        private readonly int _maxLogs;
        private int _logCount;

        public StopLogger(int maxLogs) => _maxLogs = maxLogs;

        public void Log(string message, ConsoleColor color = ConsoleColor.Cyan)
        {
            if (++_logCount > _maxLogs)
                throw new InvalidOperationException("Max number of logs exceeded");
            var defaultColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = defaultColor;
        }
    }
}
