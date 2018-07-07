namespace Trustcoin.Story
{
    public interface Factory
    {
        Artefact CreateArtefact(string name);
        PrivateAccount CreatePrivateAccount(string name);
        Transaction<TContent> CreateTransaction<TContent>(TContent content);
    }
}