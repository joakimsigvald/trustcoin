using System;
using Trustcoin.Story.Messages;
using Trustcoin.Story.Types;

namespace Trustcoin.Story
{
    public interface Peer : Person
    {
        ConfidenceValue GetMoney(int targetId, Guid? beforeTransaction, params int[] whosAsking);
        void Receive(Transaction<GotArtefact> gotArtefact);
        void Receive(Transaction<LostArtefact> lostArtefact);
        void Receive(Transaction<Endorcement> endorcement);
        void Receive(Transaction<Compliment> compliment);
        void Receive(Transaction<MoneyTransfer> moneyTransfer);
        Transaction<MoneyTransfer> ConfirmMoneyTransfer(Transaction<MoneyTransfer> transaction);
    }
}