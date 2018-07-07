using Trustcoin.Story;

namespace Trustcoin.Main.Commands
{
    public class Compliment : Command
    {
        public void Execute(params string[] args)
        {
            var accountName = args.NthOrDefault(0) ?? Program.Ask("Name account");
            var artefactName = args.NthOrDefault(1) ?? Program.Ask("Name artefact");
            var account = Program.TestBench.GetAccount(accountName);
            var artefact = Program.TestBench.GetArtefact(artefactName);
            account.Compliment(artefact);
        }

        public string Name => "like";

        public char Short => '^';
    }
}