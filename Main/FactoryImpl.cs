using Trustcoin.Story;

namespace Trustcoin.Main
{
    public class FactoryImpl : Factory
    {
        private readonly InstantNetwork _network;
        private int _nextId = 1;

        public FactoryImpl(InstantNetwork network) => _network = network;

        public Artefact CreateArtefact(string name)
            => new Artefact(_nextId++, name);

        public PrivateAccount CreatePrivateAccount(string name)
        {
            var account = new PrivateAccount(_nextId++, name, this, _network);
            _network.Register(account);
            return account;
        }

        public Transaction<TContent> CreateTransaction<TContent>(int signature, TContent content)
            => new Transaction<TContent>(signature, content);
    }
}