namespace Trustcoin.Story.Messages
{
    public struct MoneyTransferInitiated
    {
        public MoneyTransferInitiated(int senderId, float amount)
        {
            SenderId = senderId;
            Amount = amount;
        }

        public int SenderId { get; }
        public float Amount { get; }
    }
}