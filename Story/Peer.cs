using System;
using Trustcoin.Story.Messages;
using Trustcoin.Story.Types;

namespace Trustcoin.Story
{
    public interface Peer : Person
    {
        /*
        void Endorced(Peer endorcer, Peer receiver, Guid transaction);
        void Complimented(Peer endorcer, Artefact artefact, Guid transaction);
        void GotArtefact(Peer person, Artefact artefact);
        void LostArtefact(Peer person, Artefact artefact);
        */
        ConfidenceValue GetMoney(int targetId, Guid? beforeTransaction, params int[] whosAsking);
        void Receive(GotArtefact gotArtefact);
        void Receive(LostArtefact lostArtefact);
        void Receive(Endorcement endorcement, Guid transaction);
        void Receive(Compliment compliment, Guid transaction);
    }
}