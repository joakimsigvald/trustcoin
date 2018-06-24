using System;
using System.Collections.Generic;
using System.Linq;

namespace Trustcoin.Story
{
    public class PrivateAccount : Account, Peer
    {
        private readonly Logger _logger;
        private readonly Dictionary<Peer, PersonData> _peers = new Dictionary<Peer, PersonData>();
        private readonly List<Artefact> _myArtefacts = new List<Artefact>();
        private readonly List<Artefact> _knownArtefacts = new List<Artefact>();

        public PrivateAccount(int id, string name, Logger logger)
        {
            _logger = logger;
            Id = id;
            Name = name;
        }

        public int Id { get; }
        public string Name { get; }
        private float GeneralTrust { get; } = 0.2f;
        private float EndorcementTrustFactor { get; } = 0.1f;
        private float ArtefactEndorcementTrustFactor { get; } = 0.02f;
        private float ArtefactDisputeDoubtFactor { get; } = 0.1f;

        public void AddArtefact(Artefact artefact)
        {
            artefact.Owner = this;
            _myArtefacts.Add(artefact);
            InformPeers(peer => peer.GotArtefact(this, artefact));
        }

        public void Endorce(Peer receiver)
        {
            var receiverData = GetInitializedData(receiver);
            if (receiverData.IsEndorced) return;

            receiverData.IsEndorced = true;
            receiverData.Grace(EndorcementTrustFactor);
            InformPeers(peer => peer.Endorced(this, receiver), receiver);
        }

        public void Compliment(Artefact artefact)
        {
            var receiver = artefact.Owner;
            var receiverData = GetInitializedData(receiver);
            var artefactData = receiverData.GetArtefact(artefact.Id);
            if (artefactData.IsEndorced) return;

            artefactData.IsEndorced = true;
            receiverData.Grace(ArtefactEndorcementTrustFactor);
            InformPeers(peer => peer.Complimented(this, artefact), receiver);
        }

        public float? GetTrust(Peer person)
            => GetData(person).Trust;

        public float? GetMoney(Peer person)
            => GetData(person).Money;

        public void GotArtefact(Peer claimer, Artefact artefact)
        {
            var knownArtefact = _knownArtefacts.Get(artefact);
            var owner = knownArtefact?.Owner ?? claimer;
            if (claimer.Equals(owner))
                RegisterArtefact(claimer, artefact);
            else
                DisputeArtefact(owner, claimer, artefact);
        }

        public void Endorced(Peer endorcer, Peer receiver)
        {
            var relation = GetData(endorcer).GetRelation(receiver.Id);
            if (relation.IsEndorcer)
                return;
            relation.IsEndorcer = true;
            relation.Strengthen();
            AddMoneyToEndorcementReceiver(endorcer, receiver, relation);
        }

        public void Complimented(Peer endorcer, Artefact artefact)
        {
            var owner = artefact.Owner;
            var endorcerData = GetData(endorcer);
            var relation = GetData(endorcer).GetRelation(owner.Id);
            var artefactData = endorcerData.GetArtefact(artefact.Id);
            if (artefactData == null || artefactData.IsEndorcedBy(endorcer))
                return;
            artefactData.Endorced(endorcer);
            relation.Strengthen();
            AddMoneyToEndorcementReceiver(endorcer, owner, relation);
        }

        public override string ToString()
            => $"{Name}: {ShowReputation()}, {ShowMoney()}, {ShowArtefacts()}, {ShowPeers()}";

        public bool Equals(Person other)
            => other.Id == Id;

        public override bool Equals(object obj)
            => Equals(obj as Person);

        public override int GetHashCode()
            => Id;

        private void RegisterArtefact(Peer claimer, Artefact artefact)
        {
            _knownArtefacts.AddUnique(artefact);
            GetData(claimer).AddArtefact(artefact.Id, new ArtefactData());
        }

        private void DisputeArtefact(Peer owner, Peer claimer, Artefact artefact)
        {
            var ownerData = GetInitializedData(owner);
            var claimerData = GetInitializedData(claimer);
            ownerData.Doubt(ArtefactDisputeDoubtFactor);
            claimerData.Doubt(ArtefactDisputeDoubtFactor);
            if (claimerData.Trust > ownerData.Trust)
                claimerData.AddArtefact(artefact.Id, ownerData.RemoveArtefact(artefact.Id));
        }

        private void InformPeers(Action<Peer> inform, params Peer[] excludedPeers)
        {
            GetPeers(excludedPeers).ForEach(inform);
        }

        private PersonData GetInitializedData(Peer person)
        {
            var data = GetData(person);
            data.Trust = data.Trust
                         ?? ComputeInitialTrust(person);
            data.Money = data.Money ?? ComputeInitialMoney(person);
            return data;
        }

        private PersonData GetData(Peer person)
        {
            if (person.Equals(this))
                throw new ArgumentException("Cannot have self as peer");
            return _peers.TryGetValue(person, out var data)
                ? data
                : (_peers[person] = new PersonData());
        }

        private void AddMoneyToEndorcementReceiver(Peer endorcer, Peer receiver, RelationData relation)
        {
            var endorcerData = GetInitializedData(endorcer);
            var receiverData = GetInitializedData(receiver);
            receiverData.AddEndorcementMoney(endorcerData, relation);
        }

        private float ComputeInitialMoney(Peer receiver)
            => EstimateMoney(receiver, this) ?? 0;

        private string ShowReputation()
            => $"<3: {EstimateReputation(this)}";

        private string ShowMoney()
            => $"$: {EstimateMoney(this)}";

        private string ShowPeers()
            => $"knows: ({string.Join(',', GetPeers().Select(p => p.Name))})";

        private string ShowArtefacts()
            => $"has: ({string.Join(',', _myArtefacts.Select(a => a.Name))})";

        private float? EstimateReputation(Peer target, params Peer[] whosAsking)
        {
            _logger.Log(EstimationFootprint(nameof(EstimateReputation), target, whosAsking));
            return GetMedian(target, peer => peer.GetTrust(target), whosAsking);
        }

        private float ComputeInitialTrust(Peer target)
            => GetMedian(target, peer => GetData(peer).Trust * peer.GetTrust(target))
               ?? GeneralTrust;

        private float? EstimateMoney(Peer target, params Peer[] whosAsking)
        {
            _logger.Log(EstimationFootprint(nameof(EstimateMoney), target, whosAsking));
            return GetMedian(target, peer => peer.GetMoney(target), whosAsking);
        }

        private string EstimationFootprint(string method, Peer target, params Peer[] whosAsking)
            => Footprint(method, target.Name, whosAsking);

        private string Footprint(string method, string subject, params Peer[] whosAsking)
            => $"{method}: {Name}=>{subject} ({string.Join(',', whosAsking.Select(wa => wa.Name))})";

        private float? GetMedian(Peer target, Func<Peer, float?> getValue, params Peer[] whosAsking)
            => GetPeers(whosAsking.Concat(new[] {this, target}).ToArray()).Select(p => GetWeightedValue(p, getValue))
                .ToList()
                .Median();

        private IEnumerable<Peer> GetPeers(params Peer[] excludedPeers)
            => _peers.Keys.Except(excludedPeers);

        private (float, float?) GetWeightedValue(Peer peer, Func<Peer, float?> getValue)
            => GetWeightedValue(GetData(peer).Trust, peer, getValue);

        private static (float, float?) GetWeightedValue(float? weight, Peer peer, Func<Peer, float?> getValue)
            => weight.HasValue
                ? (weight.Value, getValue(peer))
                : (0f, (float?) null);
    }
}