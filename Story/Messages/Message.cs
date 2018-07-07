namespace Trustcoin.Story.Messages
{
    public struct Message<TContent>
    {
        public Message(int receiverId, Transaction<TContent> transaction)
        {
            ReceiverId = receiverId;
            Transaction = transaction;
        }

        public int ReceiverId { get; }
        public Transaction<TContent> Transaction { get; }
    }
}