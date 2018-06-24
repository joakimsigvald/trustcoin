namespace Trustcoin.Story
{
    public interface Peer : Person
    {
        void Endorced(Peer endorcer, Peer receiver);
        void Complimented(Peer endorcer, Artefact artefact);
        void GotArtefact(Peer person, Artefact artefact);
        void LostArtefact(Peer person, Artefact artefact);
    }
}