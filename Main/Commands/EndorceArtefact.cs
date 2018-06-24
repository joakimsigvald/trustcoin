using System.Linq;
using Trustcoin.Story;

namespace Trustcoin.Main.Commands
{
    public class EndorceArtefact : Command
    {
        public void Execute(params string[] args)
        {
            var endorcerName = args.FirstOrDefault() ?? Program.Ask("Name endorcer");
            var artefactName = args.SecondOrDefault() ?? Program.Ask("Name artefact");
            var endorcer = Program.Network.Get(endorcerName);
            var artefact = Program.Network.GetArtefact(artefactName);
            endorcer.Compliment(artefact);
        }

        public string Name => "like";

        public char Short => '^';
    }
}