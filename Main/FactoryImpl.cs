using Trustcoin.Story;

namespace Trustcoin.Main
{
    public class FactoryImpl : Factory
    {
        private int _nextId = 1;

        public Artefact CreateArtefact(string name)
            => new Artefact(_nextId++, name);

        public PrivateAccount CreatePrivateAccount(string name)
            => new PrivateAccount(_nextId++, name, this);
    }
}