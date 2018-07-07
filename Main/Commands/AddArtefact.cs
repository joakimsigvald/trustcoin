using Trustcoin.Story;

namespace Trustcoin.Main.Commands
{
    public class AddArtefact : Command
    {
        public void Execute(params string[] args)
        {
            var personName = args.NthOrDefault(0) ?? Program.Ask("Name person");
            var artefactName = args.NthOrDefault(1) ?? Program.Ask("Write artefact name");
            Program.TestBench.AddArtefact(personName, artefactName);
        }

        public string Name => "add";

        public char Short => '+';
    }
}