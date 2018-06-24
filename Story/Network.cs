using System.Collections.Generic;
using System.Linq;

namespace Trustcoin.Story
{
    public class Network
    {
        private readonly Logger _logger;
        private static int _nextId = 1;
        private readonly List<PrivateAccount> _accounts = new List<PrivateAccount>();
        private readonly List<Artefact> _artefacts = new List<Artefact>();

        public Network(Logger logger) => _logger = logger;

        public void AddPerson(string name)
        {
            _accounts.Add(new PrivateAccount(_nextId++, name, _logger));
        }

        public IEnumerable<Account> Accounts => _accounts;

        public PrivateAccount Get(string name)
            => _accounts.Find(n => n.Name == name);

        public void AddArtefact(string personName, string artefactName)
        {
            var artefact = new Artefact(_nextId++, artefactName);
            Get(personName).AddArtefact(artefact);
            _artefacts.Add(artefact);
        }

        public Artefact GetArtefact(string artefactName)
            => _artefacts.SingleOrDefault(a => a.Name == artefactName);
    }
}
