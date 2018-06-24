using System.Collections.Generic;

namespace Trustcoin.Story
{
    public interface Account : Person
    {
        void Endorce(Peer person);
        void Compliment(Artefact artefact);
        void AddArtefact(Artefact artefact);
        void RemoveArtefact(Artefact artefact);
        IEnumerable<Artefact> SplitArtefact(Artefact artefact, params string[] newNames);
    }
}