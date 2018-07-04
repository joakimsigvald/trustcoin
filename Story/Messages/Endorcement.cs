namespace Trustcoin.Story.Messages
{
    public struct Endorcement
    {
        public Endorcement(int endorcerId, int receiverId)
        {
            EndorcerId = endorcerId;
            ReceiverId = receiverId;
        }

        public int EndorcerId { get; set; }
        public int ReceiverId { get; set; }
    }
}