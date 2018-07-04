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
            var account = Program.TestBench.GetAccount(sourceName);
            var peer = Program.TestBench.GetPeer(targetName);
            account.Endorce(peer.Id);
        }

        public string Name => "endorce";

        public char Short => '¨';
    }
}