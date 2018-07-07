using System.Collections.Generic;
using System.Linq;
using Trustcoin.Story;

namespace Trustcoin.Main
{
    public class TestBench
    {
        private readonly Factory _factory;
        private readonly List<PrivateAccount> _accounts = new List<PrivateAccount>();
        private readonly List<Artefact> _artefacts = new List<Artefact>();

        public TestBench(Factory factory) => _factory = factory;

        public void AddPerson(string name)
        {
            _accounts.Add(_factory.CreatePrivateAccount(name));
        }

        public IEnumerable<Account> Accounts => _accounts;

        public Account GetAccount(string name)
            => Get(name);

        public Peer GetPeer(string name)
            => Get(name);

        public Artefact GetArtefact(string artefactName)
            => _artefacts.SingleOrDefault(a => a.Name == artefactName);

        public void AddArtefact(string personName, string artefactName)
        {
            var artefact = _factory.CreateArtefact(artefactName);
            GetAccount(personName).AddArtefact(artefact);
            _artefacts.Add(artefact);
        }

        public void RemoveArtefact(string personName, string artefactName)
        {
            var artefact = GetArtefact(artefactName);
            GetAccount(personName).RemoveArtefact(artefact);
            _artefacts.Remove(artefact);
        }

        public void SplitArtefact(string personName, string artefactName, string[] newNames)
        {
            var artefact = GetArtefact(artefactName);
            var account = GetAccount(personName);
            var newArtefacts = account.SplitArtefact(artefact, newNames);
            _artefacts.Remove(artefact);
            newArtefacts.ForEach(_artefacts.Add);
        }

        private PrivateAccount Get(string name)
            => _accounts.Find(n => n.Name == name);

        public void TransferMoney(string senderName, string receiverName, float amount)
        {
            var sender = GetAccount(senderName);
            var receiver = GetAccount(receiverName);
            sender.SendMoney(receiver.Id, amount);
        }
    }
}
