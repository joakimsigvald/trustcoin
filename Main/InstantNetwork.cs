using System;
using System.Collections.Generic;
using Trustcoin.Story;
using Trustcoin.Story.Messages;
using Trustcoin.Story.Types;

namespace Trustcoin.Main
{
    public class InstantNetwork : Network
    {
        private readonly IDictionary<int, Peer> _peers = new Dictionary<int, Peer>();

        public void Register(Peer peer)
            => _peers[peer.Id] = peer;

        public void Send<TContent>(Message<TContent> message)
        {
            var peer = _peers[message.ReceiverId];
            switch (message.Transaction.Content)
            {
                case Transaction<GotArtefact> g:
                    peer.Receive(g);
                    break;
                case Transaction<LostArtefact> l:
                    peer.Receive(l);
                    break;
                case Transaction<Endorcement> e:
                    peer.Receive(e);
                    break;
                case Transaction<Compliment> c:
                    peer.Receive(c);
                    break;
                case Transaction<MoneyTransfer> m:
                    peer.Receive(m);
                    break;
            }
        }

        public ConfidenceValue GetMoney(int perspective, int target, Guid? beforeTransaction, int[] whosAsking)
        {
            var peer = _peers[perspective];
            return peer.GetMoney(target, beforeTransaction, whosAsking);
        }

        public string GetName(int peerId)
            => _peers[peerId].Name;

        public Transaction<MoneyTransfer> ConfirmTransaction(int initiatorId, int receiverId, Transaction<MoneyTransfer> transaction)
        {
            var peer = _peers[receiverId];
            return peer.ConfirmMoneyTransfer(transaction);
        }
    }
}