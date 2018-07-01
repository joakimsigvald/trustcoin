using System;
using System.Collections.Generic;
using System.Linq;

namespace Trustcoin.Story
{
    public class PrivateAccount : Account, Peer
    {
        private readonly Factory _factory;
        private readonly Dictionary<Peer, PersonData> _peers = new Dictionary<Peer, PersonData>();
        private readonly List<Artefact> _myArtefacts = new List<Artefact>();
        private readonly List<Artefact> _knownArtefacts = new List<Artefact>();

        public PrivateAccount(int id, string name, Factory factory)
        {
            _factory = factory;
            Id = id;
            Name = name;
        }

        public int Id { get; }
        public string Name { get; }
        private float EndorcementTrustFactor { get; } = 0.1f;
        private float ArtefactEndorcementTrustFactor { get; } = 0.02f;
        private float ArtefactDisputeDoubtFactor { get; } = 0.1f;
        private float OutlierThreshold { get; } = 0.1f;
        private float OutlierDoubtFactor { get; } = 0.05f;
        
        public void AddArtefact(Artefact artefact)
        {
            artefact.Owner = this;
            _myArtefacts.Add(artefact);
            InformPeers(peer => peer.GotArtefact(this, artefact));
        }

        public void RemoveArtefact(Artefact artefact)
        {
            artefact.Owner = null;
            _myArtefacts.Remove(artefact);
            InformPeers(peer => peer.LostArtefact(this, artefact));
        }

        public IEnumerable<Artefact> SplitArtefact(Artefact artefact, params string[] newNames)
        {
            RemoveArtefact(artefact);
            var newArtefacts = newNames
                .Select(_factory.CreateArtefact)
                .ToArray();
            newArtefacts
                .ForEach(AddArtefact);
            return newArtefacts;
        }

        public void Endorce(Peer receiver)
        {
            var receiverData = GetData(receiver);
            if (receiverData.IsEndorced) return;

            receiverData.IsEndorced = true;
            receiverData.Grace(EndorcementTrustFactor);
            InformPeers(peer => peer.Endorced(this, receiver), receiver);
        }

        public void Compliment(Artefact artefact)
        {
            var receiver = artefact.Owner;
            var receiverData = GetData(receiver);
            var artefactData = receiverData.GetArtefact(artefact.Id);
            if (artefactData.IsEndorced) return;

            artefactData.IsEndorced = true;
            receiverData.Grace(ArtefactEndorcementTrustFactor);
            InformPeers(peer => peer.Complimented(this, artefact), receiver);
        }

        public (float, float) GetMoney(Peer person)
            => GetData(person).Money;

        public void GotArtefact(Peer claimer, Artefact artefact)
        {
            var knownArtefact = _knownArtefacts.Get(artefact);
            var owner = knownArtefact?.Owner ?? claimer;
            if (claimer.Equals(owner))
                RegisterArtefact(claimer, artefact);
            else
                QuestionOwnership(owner, claimer, artefact);
        }

        public void LostArtefact(Peer claimer, Artefact artefact)
        {
            var knownArtefact = _knownArtefacts.Get(artefact);
            var owner = knownArtefact?.Owner ?? claimer;
            if (!claimer.Equals(owner))
                owner = QuestionOwnership(owner, claimer, artefact);
            UnregisterArtefact(owner, artefact);
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
            var relation = GetData(endorcer).GetRelation(owner.Id);
            var artefactData = GetData(owner).GetArtefact(artefact.Id);
            if (artefactData == null || artefactData.IsEndorcedBy(endorcer))
                return;
            artefactData.Endorced(endorcer);
            relation.Strengthen();
            AddMoneyToEndorcementReceiver(endorcer, owner, relation);
        }

        public override string ToString()
            => $"{Name}: {ShowMoney()}, {ShowArtefacts()}, {ShowPeers()}";

        public bool Equals(Person other)
            => other.Id == Id;

        public override bool Equals(object obj)
            => Equals(obj as Person);

        public override int GetHashCode()
            => Id;

        private void RegisterArtefact(Peer claimer, Artefact artefact)
        {
            _knownArtefacts.AddUnique(artefact);
            GetData(claimer).AddArtefact(artefact.Id, new ArtefactData(artefact));
        }

        private void UnregisterArtefact(Peer owner, Artefact artefact)
        {
            _knownArtefacts.Remove(artefact);
            GetData(owner).RemoveArtefact(artefact.Id);
        }

        private Peer QuestionOwnership(
            Peer owner,
            Peer claimer,
            Artefact artefact)
        {
            var ownerData = GetData(owner);
            var claimerData = GetData(claimer);
            var leastTrustedData = new[] {claimerData, ownerData}.OrderBy(data => data.Trust).First();
            if (leastTrustedData == ownerData)
                ChangeOwner(ownerData, claimer, claimerData, artefact);
            leastTrustedData.Doubt(ArtefactDisputeDoubtFactor);
            return artefact.Owner;
        }

        private static void ChangeOwner(
            PersonData ownerData, 
            Peer claimer, 
            PersonData claimerData, 
            Artefact artefact)
        {
            artefact.Owner = claimer;
            claimerData.AddArtefact(artefact.Id, ownerData.RemoveArtefact(artefact.Id));
        }

        private void InformPeers(Action<Peer> inform, params Peer[] excludedPeers)
        {
            GetPeers(excludedPeers).ForEach(inform);
        }

        private PersonData GetData(Peer person)
        {
            if (person.Equals(this))
                throw new ArgumentException("Cannot have self as peer");
            return _peers.SafeGetValue(person) ?? (_peers[person] = new PersonData());
        }

        private void AddMoneyToEndorcementReceiver(Peer endorcer, Peer receiver, RelationData relation)
        {
            var addition = (GetData(endorcer).Trust, 1 - relation.Strength);
            GetData(receiver).AddMoney(addition, () => EstimateMoney(receiver, this));
        }

        private string ShowMoney()
            => $"$: {EstimateMoney(this).Item2}";

        private string ShowPeers()
            => $"knows: ({string.Join(',', GetPeers().Select(ShowPeer))})";

        private string ShowPeer(Peer peer)
        {
            var peerData = GetData(peer);
            return $"{peer.Name}: {peerData.Trust} [{string.Join(',', peerData.Artefacts)}]";
        }

        private string ShowArtefacts()
            => $"has: ({string.Join(',', Artefacts)})";

        private IEnumerable<Artefact> Artefacts => _myArtefacts.OrderBy(a => a.Name);

        private (float, float) EstimateMoney(Peer target, params Peer[] whosAsking) 
            => GetMedian(target, peer => peer.GetMoney(target), whosAsking);

        private (float, float) GetMedian(Peer target, Func<Peer, (float, float)> getValue, params Peer[] whosAsking)
        {
            var peerWeightedValues = GetPeers(whosAsking.Concat(new[] {this, target}).ToArray())
                .Select(p => (peer: p, weightedValue: GetWeightedValue(p, getValue)))
                .Where(x => x.weightedValue.Item1 > 0)
                .ToArray();
            var weightedValues = peerWeightedValues
                .Select(x => x.weightedValue)
                .ToArray();
            var median = weightedValues.Median();
            if (!median.HasValue)
                return (0, 0);
                var peerValues = peerWeightedValues
                    .Select(pwv => (peer: pwv.peer, value: pwv.weightedValue.Item2))
                    .ToArray();
                ReduceTrustForOutliers(peerValues, median.Value);
            var reliability = ComputeReliability(median.Value, weightedValues);
            return (reliability, median.Value);
        }

        private static float ComputeReliability(float median, IList<(float, float)> weightedVaues)
        {
            if (!weightedVaues.Any())
                return 0;
            var sumOfWeights = weightedVaues.Sum(wv => wv.Item1);
            var strength = sumOfWeights / (1 + sumOfWeights);
            if (median == 0)
                return strength;
            var weightedStandardDeviation =
                (float)Math.Sqrt(weightedVaues.Sum(wv => wv.Item1 * Math.Pow(wv.Item2 - median, 2)) / sumOfWeights);
            return strength * weightedStandardDeviation / median;
        }

        private void ReduceTrustForOutliers((Peer peer, float value)[] peerValues, float median)
        {
            var outliers = peerValues
                .Select(pv => (peer: pv.peer, dif: Difference(pv.value, median)))
                .Where(pd => pd.dif > OutlierThreshold)
                .ToArray();
            outliers.ForEach(ReduceTrustForOutlier);
        }

        private void ReduceTrustForOutlier((Peer peer, float dif) outlier)
        {
            var data = GetData(outlier.peer);
            data.Doubt(OutlierDoubtFactor * outlier.dif);
        }

        private static float Difference(float v1, float v2)
            => v1 == v2 ? 0 
            : v1 == 0 || v2 == 0 
            ? 1 
            : 1 - Math.Min(v1 / v2, v2 / v1);

        private IEnumerable<Peer> GetPeers(params Peer[] excludedPeers)
            => _peers.Keys.Except(excludedPeers).Where(p => GetData(p).Trust > 0);

        private (float, float) GetWeightedValue(Peer peer, Func<Peer, (float, float)> getValue)
            => GetWeightedValue(GetData(peer).Trust, peer, getValue);

        private static (float, float) GetWeightedValue(float? trust, Peer peer, Func<Peer, (float, float)> getValue)
        {
            if (!trust.HasValue)
                return (0, 0);
            var reliableValue = getValue(peer);
            return (reliableValue.Item1 * trust.Value, reliableValue.Item2);
        }
    }
}