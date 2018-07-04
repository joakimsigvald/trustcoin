namespace Trustcoin.Story.Messages
{
    public struct GotArtefact
    {
        public GotArtefact(int ownerId, Artefact artefact)
        {
            OwnerId = ownerId;
            Artefact = artefact;
        }

        public int OwnerId { get; set; }
        public Artefact Artefact { get; set; }
    }
}