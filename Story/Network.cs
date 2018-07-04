using System;
using System.Collections.Generic;
using Trustcoin.Story.Messages;

namespace Trustcoin.Story
{
    public interface Network
    {
        void Register(Peer peer);
        void Send<TContent>(Message<TContent> message);
        ConfidenceValue GetMoney(int perspective, int target, Guid? beforeTransaction, int[] whosAsking);
        string GetName(int peerId);
    }

    public class InstantNetwork : Network
    {
        private IDictionary<int, Peer> _peers = new Dictionary<int, Peer>();

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