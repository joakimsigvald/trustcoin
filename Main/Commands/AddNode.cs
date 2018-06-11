using System.Linq;

namespace Trustcoin.Main.Commands
{
    public class AddNode : Command
    {
        public void Execute(params string[] args)
        {
            var name = args.FirstOrDefault() ?? Program.Ask("Write name");
            Program.Network.AddNode(name);
        }

        public string Name => "add";

        public char Short => '+';
    }
}