namespace Trustcoin.Story.Messages
{
    public struct LostArtefact
    {
        public LostArtefact(int ownerId, Artefact artefact)
        {
            OwnerId = ownerId;
            Artefact = artefact;
        }

        public int OwnerId { get; set; }
        public Artefact Artefact { get; set; }
    }
}