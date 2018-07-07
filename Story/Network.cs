using System;
using Trustcoin.Story.Types;

namespace Trustcoin.Story
{
    public interface Network
    {
        void Send<TContent>(int receiverId, Transaction<TContent> message);
        ConfidenceValue GetMoney(int perspective, int target, Guid? beforeTransaction, int[] whosAsking);
        string GetName(int peerId);
    }
}