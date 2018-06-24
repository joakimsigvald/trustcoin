using System;

namespace Trustcoin.Main.Commands
{
    public class ListCommands : Command
    {
        public void Execute(params string[] args)
        {
            Program.Header2("Commands");
            foreach (var command in Program.Commands)
            {
                Console.WriteLine(command.Signature());
            }
        }

        public string Name => "help";

        public char Short => '?';
    }
}