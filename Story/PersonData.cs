using System;
using System.Collections.Generic;

namespace Trustcoin.Story
{
    internal class PersonData
    {
        private const float MaxTrust = 0.999f;

        private readonly Dictionary<int, RelationData> _relations = new Dictionary<int, RelationData>();
        private readonly Dictionary<int, ArtefactData> _artefacts = new Dictionary<int, ArtefactData>();

        internal bool IsEndorced { get; set; }

        internal float? Trust
        {
            get; set;
        }

        internal float? Money
        {
            get; set;
        }

        internal RelationData GetRelation(int id)
            => _relations.TryGetValue(id, out var nd)
                ? nd
                : _relations[id] = new RelationData();

        internal void AddEndorcementMoney(PersonData endorcerData, RelationData relation)
        {
            Money += endorcerData.GenerateEndorcementMoney(relation);
        }

        internal void Grace(float trustFactor)
        {
            Money = null;
            Trust += trustFactor * (MaxTrust - Trust);
        }

        internal void Doubt(float doubtFactor)
        {
            Trust *= 1 - doubtFactor;
        }

        private float GenerateEndorcementMoney(RelationData relation)
            => (float)Math.Sqrt(Trust.Value * (1 - relation.Strength));

        public void AddArtefact(int id, ArtefactData artefact)
        {
            _artefacts[id] = artefact;
        }

        public ArtefactData RemoveArtefact(int id)
            => _artefacts.Drop(id);

        public ArtefactData GetArtefact(int id)
            => _artefacts.SafeGetValue(id);
    }
}