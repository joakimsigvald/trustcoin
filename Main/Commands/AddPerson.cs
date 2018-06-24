using System.Linq;

namespace Trustcoin.Main.Commands
{
    public class AddPerson : Command
    {
        public void Execute(params string[] args)
        {
            var name = args.FirstOrDefault() ?? Program.Ask("Write name");
            Program.Network.AddPerson(name);
        }

        public string Name => "add";

        public char Short => '+';
    }
}