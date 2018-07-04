using System;
using Trustcoin.Story.Messages;
using Trustcoin.Story.Types;

namespace Trustcoin.Story
{
    public interface Network
    {
        void Register(Peer peer);
        void Send<TContent>(Message<TContent> message);
        ConfidenceValue GetMoney(int perspective, int target, Guid? beforeTransaction, int[] whosAsking);
        string GetName(int peerId);
    }
}