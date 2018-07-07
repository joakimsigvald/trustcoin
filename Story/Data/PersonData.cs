using System;
using System.Collections.Generic;
using System.Linq;
using Trustcoin.Story.Types;

namespace Trustcoin.Story.Data
{
    internal class PersonData
    {
        private const float MaxAmount = 1_000_000_000;
        private const float MaxTrust = 0.999f;
        private const float ConfidenceChangeFactor = 0.1f;

        private readonly IDictionary<Guid, ConfidenceValue> _moneyBeforeTransaction =
            new Dictionary<Guid, ConfidenceValue>();

        private readonly Dictionary<int, RelationData> _relations = new Dictionary<int, RelationData>();
        private readonly Dictionary<int, ArtefactData> _artefacts = new Dictionary<int, ArtefactData>();
        private ConfidenceValue _money;

        internal PersonData(int id, string name)
        {
            Id = id;
            Name = name;
        }

        internal int Id { get; set; }
        internal string Name { get; }
        internal bool IsEndorced { get; set; }

        internal float Trust { get; private set; }

        internal RelationData GetRelation(int id)
            => _relations.TryGetValue(id, out var nd)
                ? nd
                : _relations[id] = new RelationData();

        internal IEnumerable<ArtefactData> Artefacts
            => _artefacts.Values.OrderBy(a => a.Name);


        internal float MaxTransferableAmount => _money.Confidence * MaxTransferedAmount;

        internal void AddMoney(ConfidenceValue addition, Guid transaction)
        {
            _moneyBeforeTransaction[transaction] = _money;
            var addedValue = addition.Value;
            var newValue = _money.Value + addedValue;
            var k = ConfidenceChangeFactor * addedValue / newValue;
            var newConfidence = k * addition.Confidence + (1 - k) * _money.Confidence;
            var possessableAmount = MaxAmount * newConfidence;
            var newPossessableValue = Math.Min(possessableAmount, newValue);
            _money = new ConfidenceValue(newConfidence, newPossessableValue);
        }

        internal void Grace(float trustFactor)
        {
            Trust += trustFactor * (MaxTrust - Trust);
        }

        internal void Doubt(float doubtFactor)
        {
            Trust *= 1 - doubtFactor;
        }

        internal void AddArtefact(int id, ArtefactData artefact)
        {
            _artefacts[id] = artefact;
        }

        internal ArtefactData RemoveArtefact(int id)
            => _artefacts.Drop(id);

        internal ArtefactData GetArtefact(int id)
            => _artefacts.SafeGetValue(id);

        internal void UpdateMoney(Func<ConfidenceValue> estimate)
        {
            var newEstimation = estimate();
            if (newEstimation.Confidence > _money.Confidence)
                _money = newEstimation;
        }

        internal ConfidenceValue GetMoney(Guid? beforeTransaction)
            => beforeTransaction.HasValue &&
               _moneyBeforeTransaction.TryGetValue(beforeTransaction.Value, out ConfidenceValue moneyBefore)
                ? moneyBefore
                : _money;

        private float MaxTransferedAmount => _money.Confidence * _money.Value;
    }
}