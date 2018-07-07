using System;
using Trustcoin.Story.Messages;
using Trustcoin.Story.Types;

namespace Trustcoin.Story
{
    public interface Peer : Person
    {
        ConfidenceValue GetMoney(int targetId, Guid? beforeTransaction, params int[] whosAsking);
        void Receive(Transaction<GotArtefact> _);
        void Receive(Transaction<LostArtefact> _);
        void Receive(Transaction<Endorcement> _);
        void Receive(Transaction<Compliment> _);
        void Receive(Transaction<MoneyTransferAccepted> _);
        void Receive(Transaction<MoneyTransferInitiated> _);
    }
}