using System.Linq;
using Trustcoin.Story;

namespace Trustcoin.Main.Commands
{
    public class Endorce : Command
    {
        public void Execute(params string[] args)
        {
            var sourceName = args.FirstOrDefault() ?? Program.Ask("Name source");
            var targetName = args.SecondOrDefault() ?? Program.Ask("Name target");
            var source = Program.Network.Get(sourceName);
            var target = Program.Network.Get(targetName);
            source.Endorce(target);
        }

        public string Name => "like";

        public char Short => 'd';
    }
}