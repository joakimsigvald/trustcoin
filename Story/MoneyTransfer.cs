namespace Trustcoin.Story
{
    public struct MoneyTransfer
    {
        public MoneyTransfer(int senderId, int receiverId, float amount)
        {
            SenderId = senderId;
            ReceiverId = receiverId;
            Amount = amount;
        }

        public int SenderId { get; }
        public int ReceiverId { get; }
        public float Amount { get; }
    }
}