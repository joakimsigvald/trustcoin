using System.Linq;
using Trustcoin.Story;

namespace Trustcoin.Main.Commands
{
    public class SplitArtefact : Command
    {
        public void Execute(params string[] args)
        {
            var personName = args.FirstOrDefault() ?? Program.Ask("Name person");
            var artefactName = args.SecondOrDefault() ?? Program.Ask("Write artefact name");
            var newNames = args.Length > 2 
                ? args.Skip(2).ToArray() 
                : Program.Ask("Write new names comma-separated").Split(',');
            Program.Network.SplitArtefact(personName, artefactName, newNames);
        }

        public string Name => "split";

        public char Short => '%';
    }
}