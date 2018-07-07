using System.Linq;
using Trustcoin.Story;

namespace Trustcoin.Main.Commands
{
    public class SplitArtefact : Command
    {
        public void Execute(params string[] args)
        {
            var personName = args.NthOrDefault(0) ?? Program.Ask("Name person");
            var artefactName = args.NthOrDefault(1) ?? Program.Ask("Write artefact name");
            var newNames = args.Length > 2 
                ? args.Skip(2).ToArray() 
                : Program.Ask("Write new names comma-separated").Split(',');
            Program.TestBench.SplitArtefact(personName, artefactName, newNames);
        }

        public string Name => "split";

        public char Short => '%';
    }
}