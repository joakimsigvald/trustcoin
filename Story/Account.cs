namespace Trustcoin.Story
{
    public interface Account : Person
    {
        void Endorce(Peer person);
        void Compliment(Artefact artefact);
        void AddArtefact(Artefact artefact);
    }
}