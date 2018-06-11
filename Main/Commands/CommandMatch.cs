using System.Collections.Generic;

namespace Trustcoin.Main.Commands
{
    public class CommandMatch
    {
        public Command PrimaryCommand { get; set; }
        public string[] Arguments { get; set; } = new string[0];
        public IEnumerable<Command> PostCommands { get; set; } = new Command[0];

        public static CommandMatch Create(Command command)
            => command == null
                ? null
                : new CommandMatch
                {
                    PrimaryCommand = command
                };
    }
}