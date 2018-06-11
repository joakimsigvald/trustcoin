using System;
namespace Trustcoin.Main.Commands
{
    public class Quit : Command
    {
        public void Execute(params string[] args)
        {
            Environment.Exit(0);
        }

        public string Name => "quit";

        public char Short => 'q';
    }
}