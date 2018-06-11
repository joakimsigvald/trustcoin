using System;

namespace Trustcoin.Main.Commands
{
    public class ListNodes : Command
    {
        public void Execute(params string[] args)
        {
            Program.Header2("Participants");
            foreach (var node in Program.Network.Nodes)
            {
                Console.WriteLine($"{node}");
            }
        }

        public string Name => "nodes";

        public char Short => 'l';
    }
}