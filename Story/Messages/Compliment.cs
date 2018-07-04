namespace Trustcoin.Story.Messages
{
    public struct Compliment
    {
        public Compliment(int complementerId, int artefactId)
        {
            ComplementerId = complementerId;
            ArtefactId = artefactId;
        }

        public int ComplementerId { get; set; }
        public int ArtefactId { get; set; }
    }
}