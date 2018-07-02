using System;
using System.Collections.Generic;
using System.Linq;

namespace Trustcoin.Story
{
    internal class PersonData
    {
        private const float MaxAmount = 1_000_000_000;
        private const float MaxTrust = 0.999f;
        private const float ReliabilityChangeFactor = 0.1f;

        private readonly Dictionary<int, RelationData> _relations = new Dictionary<int, RelationData>();
        private readonly Dictionary<int, ArtefactData> _artefacts = new Dictionary<int, ArtefactData>();

        internal bool IsEndorced { get; set; }

        internal float Trust
        {
            get;
            private set;
        }

        internal (float, float) Money
        {
            get;
            private set;
        }

        internal RelationData GetRelation(int id)
            => _relations.TryGetValue(id, out var nd)
                ? nd
                : _relations[id] = new RelationData();

        internal IEnumerable<ArtefactData> Artefacts
            => _artefacts.Values.OrderBy(a => a.Name);

        internal void AddMoney((float, float) addition)
        {
            var addedValue = addition.Item2;
            var newValue = Money.Item2 + addedValue;
            var k = ReliabilityChangeFactor * addedValue / newValue;
            var newReliablilty = k * addition.Item1 + (1 - k) * Money.Item1;
            var possessableAmount = MaxAmount * newReliablilty;
            var newPossessableValue = Math.Min(possessableAmount, newValue);
            Money = (newReliablilty, newPossessableValue);
        }

        internal void Grace(float trustFactor)
        {
            Money = (0, 0);
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

        public void UpdateMoney(Func<(float, float)> estimate)
        {
            var newEstimation = estimate();
            if (newEstimation.Item1 > Money.Item1)
                Money = newEstimation;
        }
    }
}