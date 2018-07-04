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
            switch (message.Content)
            {
                case GotArtefact g:
                    peer.Receive(g);
                    break;
                case LostArtefact l:
                    peer.Receive(l);
                    break;
                case Endorcement e:
                    peer.Receive(e, message.Transaction);
                    break;
                case Compliment c:
                    peer.Receive(c, message.Transaction);
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