using System;
using System.Collections.Generic;
using System.Linq;
using Trustcoin.Story.Data;
using Trustcoin.Story.Messages;
using Trustcoin.Story.Types;

namespace Trustcoin.Story
{
    /// <summary>
    /// TODO
    /// * Transactions
    /// - Transferable amount: confidence^2*possession
    /// - On each money update, possession is truncated by possessable amount
    /// * Share/Update/Set "constants"
    /// * Set/Reset trust
    /// * Negative trust
    /// * Proof of autenticity (digital signature)
    /// * Anonymous transactions through third party (transaction mediator)
    /// - transaction mediator issue tickets to both parts:
    /// - one part buys the ticket, the other part uses her ticket to retreive the same amount
    /// - the transactino mediator may charge a fee (based on percentage of amount, absolute or combination)
    /// - the algorithm automatically chooses the most trusted/cheapest mediato(s) common to both parts og the transaction
    /// * Test coverage
    /// * Bigger scenarious
    /// * Fraud scenarious
    /// * Three types of actors:
    /// - private: can make transactions
    /// - regional transaction mediator: Can issue tickets to two known actors, one is sold to one part and the other can be used to retreive the same amount of money from the partner
    /// - global transaction mediator: Only connects with transaction mediators and attempt to connect with all other global transaction mediators
    /// - this way the longest chain between two actors is: private-regional-global-global-regional-private
    /// - one global transaction mediator can issue tickets that is delegated through the chain to the private actors
    /// * Binary dimensions to position all actors, to facilitate finding common mediators
    /// - purpose: To facilitate search of common chain of mediators between two parties
    /// - objective: make actors spread out evenly, but make actors that interact alot cluster together, so that distance shows amount of interaction
    /// - 128 binary dimensions gives an enourmeous information space where the nodes are spread out (2^128 voxels)
    /// - on each interaction, a random dimension is flipped for the initiator, then a random common dimension where they are different is selected and flipped for the initiator
    /// * Implement real digital signatures
    /// * Handle synchronizatino - single threaded accounts
    /// </summary>

    public class PrivateAccount : Account, Peer
    {
        private readonly List<Transaction> _handledTransactions = new List<Transaction>();
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
        private float MoneyTransferTrustFactor { get; } = 0.1f;
        private float ArtefactEndorcementTrustFactor { get; } = 0.02f;
        private float ArtefactDisputeDoubtFactor { get; } = 0.1f;
        private float InvalidTransferDoubtFactor { get; } = 0.1f;
        private float OutlierThreshold { get; } = 0.1f;
        private float OutlierDoubtFactor { get; } = 0.05f;
        
        public void AddArtefact(Artefact artefact)
        {
            artefact.OwnerId = Id;
            _myArtefacts.Add(artefact);
            InformPeers(_factory.CreateTransaction(new GotArtefact(Id, artefact)));
        }

        public void RemoveArtefact(Artefact artefact)
        {
            artefact.OwnerId = null;
            _myArtefacts.Remove(artefact);
            InformPeers(_factory.CreateTransaction(new LostArtefact(Id, artefact)));
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

        public void SendMoney(int receiverId, float amount)
        {
            var transfer = new MoneyTransfer(Id, receiverId, amount);
            var transaction = _factory.CreateTransaction(transfer);
            var signedTransaction = transaction.Sign(Id); 
            var confirmedTransaction = _network.ConfirmTransaction(Id, receiverId, signedTransaction);
            if (!confirmedTransaction.IsSignedBy(receiverId))
                throw new ReceiverDeniedTransaction();
            InformPeers(confirmedTransaction);
            RetrieveData(receiverId).Grace(MoneyTransferTrustFactor);
        }

        public void Endorce(int receiverId)
        {
            var receiverData = GetData(receiverId);
            if (receiverData.IsEndorced) return;

            receiverData.IsEndorced = true;
            receiverData.Grace(EndorcementTrustFactor);
            InformPeers(_factory.CreateTransaction(new Endorcement(Id, receiverId)));
        }

        public void Compliment(Artefact artefact)
        {
            var receiverId = artefact.KnownOwnerId;
            var receiverData = GetData(receiverId);
            var artefactData = receiverData.GetArtefact(artefact.Id);
            if (artefactData.IsEndorced) return;

            artefactData.IsEndorced = true;
            receiverData.Grace(ArtefactEndorcementTrustFactor);
            InformPeers(_factory.CreateTransaction(new Compliment(Id, artefact.Id)));
        }

        public ConfidenceValue GetMoney(int targetId, Guid? beforeTransaction = null, params int[] whosAsking)
            => RetrieveData(targetId, beforeTransaction, whosAsking)
            .GetMoney(beforeTransaction);

        public void Receive(Transaction<GotArtefact> _)
        {
            var gotArtefact = _.Content;
            var knownArtefact = _knownArtefacts.Get(gotArtefact.Artefact);
            var ownerId = knownArtefact?.OwnerId ?? gotArtefact.OwnerId;
            if (ownerId == gotArtefact.OwnerId)
                RegisterArtefact(gotArtefact.OwnerId, gotArtefact.Artefact);
            else
                QuestionOwnership(ownerId, gotArtefact.OwnerId, gotArtefact.Artefact);
        }

        public void Receive(Transaction<LostArtefact> _)
        {
            var lostArtefact = _.Content;
            var knownArtefact = _knownArtefacts.Get(lostArtefact.Artefact);
            var ownerId = knownArtefact?.OwnerId ?? lostArtefact.OwnerId;
            if (ownerId != lostArtefact.OwnerId)
                ownerId = QuestionOwnership(ownerId, lostArtefact.OwnerId, lostArtefact.Artefact);
            if (ownerId == lostArtefact.OwnerId)
                UnregisterArtefact(ownerId, lostArtefact.Artefact);
        }

        public void Receive(Transaction<Endorcement> _)
        {
            var endorcement = _.Content;
            if (endorcement.ReceiverId == Id)
                return;
            var relation = GetData(endorcement.EndorcerId).GetRelation(endorcement.ReceiverId);
            if (relation.IsEndorcer)
                return;
            relation.IsEndorcer = true;
            relation.Strengthen();
            AddMoneyToEndorcementReceiver(endorcement.EndorcerId, endorcement.ReceiverId, relation, _.Id);
        }

        public void Receive(Transaction<Compliment> _)
        {
            var compliment = _.Content;
            var artefact = _knownArtefacts.SingleOrDefault(art => art.Id == compliment.ArtefactId);
            if (artefact?.OwnerId == null || artefact.OwnerId == Id)
                return;
            var ownerId = artefact.KnownOwnerId;
            var relation = GetData(compliment.ComplementerId).GetRelation(ownerId);
            var artefactData = GetData(ownerId).GetArtefact(compliment.ArtefactId);
            if (artefactData == null || artefactData.IsComplementedBy(compliment.ComplementerId))
                return;
            artefactData.Complimented(compliment.ComplementerId);
            relation.Strengthen();
            AddMoneyToEndorcementReceiver(compliment.ComplementerId, ownerId, relation, _.Id);
        }

        public void Receive(Transaction<MoneyTransfer> _)
        {
            if (_handledTransactions.Contains(_))
                return;
            _handledTransactions.Add(_);
            var transfer = _.Content;
            if (!_.IsSignedBy(transfer.SenderId) || !_.IsSignedBy(transfer.ReceiverId))
                return;
            if (transfer.SenderId == Id)
                RegisterMoneyTransferSent(transfer, _);
            else if (transfer.ReceiverId == Id)
                RegisterMoneyTransferReceived(transfer, _);
            else RegisterThirdPartMoneyTransfer(transfer, _);
        }

        private void RegisterThirdPartMoneyTransfer(MoneyTransfer transfer, Transaction _)
        {
            var senderData = RetrieveData(transfer.SenderId);
            var receiverData = RetrieveData(transfer.ReceiverId);
            if (senderData.MaxTransferableAmount < transfer.Amount)
            {
                senderData.Doubt(InvalidTransferDoubtFactor * (1 - senderData.MaxTransferableAmount / transfer.Amount));
                return;
            }
            senderData.GetRelation(transfer.ReceiverId).Strengthen();
            receiverData.GetRelation(transfer.SenderId).Strengthen();
            senderData.AddMoney(new ConfidenceValue(1, -transfer.Amount), _.Id);
            receiverData.AddMoney(new ConfidenceValue(1, transfer.Amount), _.Id);
        }

        private void RegisterMoneyTransferReceived(MoneyTransfer transfer, Transaction _)
        {
            RetrieveData(transfer.SenderId).AddMoney(new ConfidenceValue(1, -transfer.Amount), _.Id);
        }

        private void RegisterMoneyTransferSent(MoneyTransfer transfer, Transaction _)
        {
            RetrieveData(transfer.ReceiverId).AddMoney(new ConfidenceValue(1, transfer.Amount), _.Id);
        }

        public Transaction<MoneyTransfer> ConfirmMoneyTransfer(Transaction<MoneyTransfer> transaction)
        {
            var transferAccepted = transaction.IsSignedBy(transaction.Content.SenderId)
                                   &&  IsValidTransfer(transaction.Content);
            if (!transferAccepted)
                return transaction;
            var signedTransaction = transaction.Sign(Id);
            InformPeers(signedTransaction);
            return signedTransaction;
        }

        private bool IsValidTransfer(MoneyTransfer transfer)
            => RetrieveData(transfer.SenderId).MaxTransferableAmount >= transfer.Amount;

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

        private void InformPeers<TContent>(Transaction<TContent> transaction)
        {
            GetPeers().ForEach(peer => _network.Send(new Message<TContent>(peer, transaction)));
        }

        private void AddMoneyToEndorcementReceiver(int endorcerId, int receiverId, RelationData relation, Guid transaction)
        {
            var addition = new ConfidenceValue(GetData(endorcerId).Trust, 1 - relation.Strength);
            var data = RetrieveData(receiverId, transaction);
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

        private PersonData RetrieveData(int peerId, Guid? beforeTransaction = null, params int[] whosAsking)
        {
            var data = GetData(peerId);
            data.UpdateMoney(() => EstimateMoney(peerId, beforeTransaction, whosAsking));
            return data;
        }

        private PersonData GetData(int peerId)
        {
            if (peerId == Id)
                throw new ArgumentException("Cannot have self as peer");
            return _peers.SafeGetValue(peerId) ?? (_peers[peerId] = new PersonData(_network.GetName(peerId)));
        }

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