using System.Linq;
using Trustcoin.Story;

namespace Trustcoin.Main.Commands
{
    public class Compliment : Command
    {
        public void Execute(params string[] args)
        {
            var accountName = args.FirstOrDefault() ?? Program.Ask("Name account");
            var artefactName = args.SecondOrDefault() ?? Program.Ask("Name artefact");
            var account = Program.Network.GetAccount(accountName);
            var artefact = Program.Network.GetArtefact(artefactName);
            account.Compliment(artefact);
        }

        public string Name => "like";

        public char Short => '^';
    }
}