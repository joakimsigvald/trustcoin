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

        public void Send<TContent>(int receiverId, Transaction<TContent> transaction)
        {
            var peer = _peers[receiverId];
            switch (transaction.Content)
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
                case Transaction<MoneyTransferInitiated> i:
                    peer.Receive(i);
                    break;
                case Transaction<MoneyTransferAccepted> a:
                    peer.Receive(a);
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
    }
}