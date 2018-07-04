using System;
using System.Collections.Generic;
using System.Linq;
using Trustcoin.Story.Messages;

namespace Trustcoin.Story
{
    /// <summary>
    /// Transferable amount: confidence^2*possession
    /// 
    /// On each money update, possession is truncated by possessable amount
    /// </summary>

    public class PrivateAccount : Account, Peer
    {
        private readonly Factory _factory;
        private readonly Network _network;
        private readonly Dictionary<int, PersonData> _peers = new Dictionary<int, PersonData>();
        private readonly List<Artefact> _myArtefacts = new List<Artefact>();
        private readonly List<Artefact> _knownArtefacts = new List<Artefact>();

        public PrivateAccount(int id, string name, Factory factory, Network network)
        {
            _factory = factory;
            _network = network;
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
            artefact.OwnerId = Id;
            _myArtefacts.Add(artefact);
            InformPeers(new GotArtefact(Id, artefact));
        }

        public void RemoveArtefact(Artefact artefact)
        {
            artefact.OwnerId = null;
            _myArtefacts.Remove(artefact);
            InformPeers(new LostArtefact(Id, artefact));
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

        public void Endorce(int receiverId)
        {
            var receiverData = GetData(receiverId);
            if (receiverData.IsEndorced) return;

            receiverData.IsEndorced = true;
            receiverData.Grace(EndorcementTrustFactor);
            InformPeers(new Endorcement(Id, receiverId));
        }

        public void Compliment(Artefact artefact)
        {
            var receiverId = artefact.KnownOwnerId;
            var receiverData = GetData(receiverId);
            var artefactData = receiverData.GetArtefact(artefact.Id);
            if (artefactData.IsEndorced) return;

            artefactData.IsEndorced = true;
            receiverData.Grace(ArtefactEndorcementTrustFactor);
            InformPeers(new Compliment(Id, artefact.Id));
        }

        public ConfidenceValue GetMoney(int targetId, Guid? beforeTransaction = null, params int[] whosAsking)
        {
            var data = GetData(targetId);
            data.UpdateMoney(() => EstimateMoney(targetId, beforeTransaction, whosAsking));
            return data.GetMoney(beforeTransaction);
        }

        public void Receive(GotArtefact _)
        {
            var knownArtefact = _knownArtefacts.Get(_.Artefact);
            var ownerId = knownArtefact?.OwnerId ?? _.OwnerId;
            if (ownerId == _.OwnerId)
                RegisterArtefact(_.OwnerId, _.Artefact);
            else
                QuestionOwnership(ownerId, _.OwnerId, _.Artefact);
        }

        public void Receive(LostArtefact _)
        {
            var knownArtefact = _knownArtefacts.Get(_.Artefact);
            var ownerId = knownArtefact?.OwnerId ?? _.OwnerId;
            if (ownerId != _.OwnerId)
                ownerId = QuestionOwnership(ownerId, _.OwnerId, _.Artefact);
            if (ownerId == _.OwnerId)
                UnregisterArtefact(ownerId, _.Artefact);
        }

        public void Receive(Endorcement _, Guid transaction)
        {
            if (_.ReceiverId == Id)
                return;
            var relation = GetData(_.EndorcerId).GetRelation(_.ReceiverId);
            if (relation.IsEndorcer)
                return;
            relation.IsEndorcer = true;
            relation.Strengthen();
            AddMoneyToEndorcementReceiver(_.EndorcerId, _.ReceiverId, relation, transaction);
        }

        public void Receive(Compliment _, Guid transaction)
        {
            var artefact = _knownArtefacts.SingleOrDefault(art => art.Id == _.ArtefactId);
            if (artefact?.OwnerId == null || artefact.OwnerId == Id)
                return;
            var ownerId = artefact.KnownOwnerId;
            var relation = GetData(_.ComplementerId).GetRelation(ownerId);
            var artefactData = GetData(ownerId).GetArtefact(_.ArtefactId);
            if (artefactData == null || artefactData.IsComplementedBy(_.ComplementerId))
                return;
            artefactData.Complimented(_.ComplementerId);
            relation.Strengthen();
            AddMoneyToEndorcementReceiver(_.ComplementerId, ownerId, relation, transaction);
        }

        public override string ToString()
            => $"{Name}: {ShowMoney()}, {ShowArtefacts()}, {ShowPeers()}";

        public bool Equals(Person other)
            => other.Id == Id;

        public override bool Equals(object obj)
            => Equals(obj as Person);

        public override int GetHashCode()
            => Id;

        private void RegisterArtefact(int claimerId, Artefact artefact)
        {
            _knownArtefacts.AddUnique(artefact);
            GetData(claimerId).AddArtefact(artefact.Id, new ArtefactData(artefact));
        }

        private void UnregisterArtefact(int ownerId, Artefact artefact)
        {
            _knownArtefacts.Remove(artefact);
            GetData(ownerId).RemoveArtefact(artefact.Id);
        }

        private int QuestionOwnership(
            int ownerId,
            int claimerId,
            Artefact artefact)
        {
            var ownerData = GetData(ownerId);
            var claimerData = GetData(claimerId);
            var leastTrustedData = new[] {claimerData, ownerData}.OrderBy(data => data.Trust).First();
            if (leastTrustedData == ownerData)
                ChangeOwner(ownerData, claimerId, claimerData, artefact);
            leastTrustedData.Doubt(ArtefactDisputeDoubtFactor);
            return artefact.KnownOwnerId;
        }

        private static void ChangeOwner(
            PersonData ownerData, 
            int claimerId, 
            PersonData claimerData, 
            Artefact artefact)
        {
            artefact.OwnerId = claimerId;
            claimerData.AddArtefact(artefact.Id, ownerData.RemoveArtefact(artefact.Id));
        }

        private void InformPeers<TContent>(TContent content)
        {
            var transaction = CreateTransaction();
            GetPeers().ForEach(peer => _network.Send(new Message<TContent>(peer, content, transaction)));
        }

        private static Guid CreateTransaction()
            => Guid.NewGuid();

        private PersonData GetData(int peerId)
        {
            if (peerId == Id)
                throw new ArgumentException("Cannot have self as peer");
            return _peers.SafeGetValue(peerId) ?? (_peers[peerId] = new PersonData(_network.GetName(peerId)));
        }

        private void AddMoneyToEndorcementReceiver(int endorcerId, int receiverId, RelationData relation, Guid transaction)
        {
            var addition = new ConfidenceValue(GetData(endorcerId).Trust, 1 - relation.Strength);
            var data = GetData(receiverId);
            data.UpdateMoney(() => EstimateMoney(receiverId, transaction));
            data.AddMoney(addition, transaction);
        }

        private string ShowMoney()
            => $"$: {EstimateMoney(Id).Value}";

        private string ShowPeers()
            => $"knows: ({string.Join(',', GetPeers().Select(ShowPeer))})";

        private string ShowPeer(int peerId)
        {
            var peerData = GetData(peerId);
            return $"{peerData.Name}: {peerData.Trust} [{string.Join(',', peerData.Artefacts)}]";
        }

        private string ShowArtefacts()
            => $"has: ({string.Join(',', Artefacts)})";

        private IEnumerable<Artefact> Artefacts => _myArtefacts.OrderBy(a => a.Name);

        private ConfidenceValue EstimateMoney(int targetId, Guid? beforeTransaction = null, params int[] whosAsking)
        {
            whosAsking = whosAsking.Append(Id).ToArray();
            return GetMedian(targetId, peer => _network.GetMoney(peer, targetId, beforeTransaction, whosAsking), whosAsking);
        }

        private ConfidenceValue GetMedian(int targetId, Func<int, ConfidenceValue> getValue, params int[] whosAsking)
        {
            var peerWeightedValues = GetPeers(whosAsking.Append(targetId).ToArray())
                .Select(p => (peer: p, weightedValue: GetWeightedValue(p, getValue)))
                .Where(x => x.weightedValue.Trust > 0)
                .ToArray();
            var weightedValues = peerWeightedValues
                .Select(x => x.weightedValue)
                .Select(twv => (twv.Trust * twv.Confidence, twv.Value))
                .ToArray();
            var median = weightedValues.Median();
            if (!median.HasValue)
                return new ConfidenceValue();
            var peerValues = peerWeightedValues
                .Select(pwv => (pwv.peer, value: (pwv.weightedValue.Confidence, pwv.weightedValue.Value)))
                .ToArray();
            ReduceTrustForOutliers(peerValues, median.Value);
            var confidence = ComputeConfidence(median.Value, weightedValues);
            return new ConfidenceValue(confidence, median.Value);
        }

        private static float ComputeConfidence(float median, IList<(float, float)> weightedVaues)
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

        private void ReduceTrustForOutliers((int peerId, (float, float) confidenceValue)[] peerValues, float median)
        {
            var outliers = peerValues
                .Select(pv => (
                pv.peerId, 
                confidence: pv.confidenceValue.Item1, 
                dif: Difference(pv.confidenceValue.Item2, median)))
                .Where(pd => pd.dif > OutlierThreshold)
                .ToArray();
            outliers.ForEach(ReduceTrustForOutlier);
        }

        private void ReduceTrustForOutlier((int peerId, float confidence, float dif) outlier)
        {
            var data = GetData(outlier.peerId);
            data.Doubt(OutlierDoubtFactor * outlier.confidence * outlier.dif);
        }

        private static float Difference(float v1, float v2)
            => v1 == v2 ? 0 
            : v1 == 0 || v2 == 0 
            ? 1 
            : 1 - Math.Min(v1 / v2, v2 / v1);

        private IEnumerable<int> GetPeers(params int[] excludedPeerIds)
            => _peers.Keys.Except(excludedPeerIds).Where(p => GetData(p).Trust > 0);

        private TrustConfidenceValue GetWeightedValue(int peerId, Func<int, ConfidenceValue> getValue)
            => GetWeightedValue(GetData(peerId).Trust, peerId, getValue);

        private static TrustConfidenceValue GetWeightedValue(float? trust, int peerId, Func<int, ConfidenceValue> getValue) 
            => !trust.HasValue 
            ? new TrustConfidenceValue() 
            : new TrustConfidenceValue(trust.Value, getValue(peerId));
    }
}