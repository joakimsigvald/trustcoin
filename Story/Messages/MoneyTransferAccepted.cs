namespace Trustcoin.Story.Messages
{
    public class MoneyTransferAccepted
    {
        public MoneyTransferAccepted(Transaction<MoneyTransferInitiated> transfer, int receiverId)
        {
            Transfer = transfer;
            ReceiverId = receiverId;
        }

        public Transaction<MoneyTransferInitiated> Transfer { get; }
        public int ReceiverId { get; }
    }
}