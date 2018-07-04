using System.Linq;

namespace Trustcoin.Main.Commands
{
    public class CreateAccount : Command
    {
        public void Execute(params string[] args)
        {
            var name = args.FirstOrDefault() ?? Program.Ask("Write name");
            Program.TestBench.AddPerson(name);
        }

        public string Name => "create";

        public char Short => '*';
    }
}