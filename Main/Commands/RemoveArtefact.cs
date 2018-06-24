using System.Linq;
using Trustcoin.Story;

namespace Trustcoin.Main.Commands
{
    public class RemoveArtefact : Command
    {
        public void Execute(params string[] args)
        {
            var personName = args.FirstOrDefault() ?? Program.Ask("Name person");
            var artefactName = args.SecondOrDefault() ?? Program.Ask("Write artefact name");
            Program.Network.RemoveArtefact(personName, artefactName);
        }

        public string Name => "remove";

        public char Short => '-';
    }
}