using System;

namespace Trustcoin.Story.Messages
{
    public struct Message<TContent>
    {
        public Message(int receiverId, TContent content, Guid transaction = default(Guid))
        {
            ReceiverId = receiverId;
            Content = content;
            Transaction = transaction;
        }

        public int ReceiverId { get; set; }
        public Guid Transaction { get; set; }
        public TContent Content { get; set; }
    }
}