using System;
using System.Collections.Generic;
using System.Linq;

namespace Trustcoin.Story
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

        internal bool IsEndorced { get; set; }

        internal float Trust { get; private set; }

        internal ConfidenceValue Money { get; private set; }

        internal RelationData GetRelation(int id)
            => _relations.TryGetValue(id, out var nd)
                ? nd
                : _relations[id] = new RelationData();

        internal IEnumerable<ArtefactData> Artefacts
            => _artefacts.Values.OrderBy(a => a.Name);

        internal void AddMoney(ConfidenceValue addition, Guid transaction)
        {
            _moneyBeforeTransaction[transaction] = Money;
            var addedValue = addition.Value;
            var newValue = Money.Value + addedValue;
            var k = ConfidenceChangeFactor * addedValue / newValue;
            var newConfidence = k * addition.Confidence + (1 - k) * Money.Confidence;
            var possessableAmount = MaxAmount * newConfidence;
            var newPossessableValue = Math.Min(possessableAmount, newValue);
            Money = new ConfidenceValue(newConfidence, newPossessableValue);
        }

        internal void Grace(float trustFactor)
        {
            Money = new ConfidenceValue();
            Trust += trustFactor * (MaxTrust - Trust);
        }

        internal void Doubt(float doubtFactor)
        {
            Trust *= 1 - doubtFactor;
        }

        public void AddArtefact(int id, ArtefactData artefact)
        {
            _artefacts[id] = artefact;
        }

        public ArtefactData RemoveArtefact(int id)
            => _artefacts.Drop(id);

        public ArtefactData GetArtefact(int id)
            => _artefacts.SafeGetValue(id);

        public void UpdateMoney(Func<ConfidenceValue> estimate)
        {
            var newEstimation = estimate();
            if (newEstimation.Confidence > Money.Confidence)
                Money = newEstimation;
        }

        public ConfidenceValue GetMoney(Guid? beforeTransaction)
            => beforeTransaction.HasValue &&
               _moneyBeforeTransaction.TryGetValue(beforeTransaction.Value, out ConfidenceValue moneyBefore)
                ? moneyBefore
                : Money;
    }
}