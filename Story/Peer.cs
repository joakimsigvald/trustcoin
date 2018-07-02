using System;

namespace Trustcoin.Story
{
    public interface Peer : Person
    {
        void Endorced(Peer endorcer, Peer receiver, Guid transaction);
        void Complimented(Peer endorcer, Artefact artefact, Guid transaction);
        void GotArtefact(Peer person, Artefact artefact);
        void LostArtefact(Peer person, Artefact artefact);
    }
}