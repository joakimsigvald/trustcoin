using System;
using System.Collections.Generic;
using System.Linq;

namespace Trustcoin.Story
{
    internal class PersonData
    {
        private const float MaxTrust = 0.999f;

        private readonly Dictionary<int, RelationData> _relations = new Dictionary<int, RelationData>();
        private readonly Dictionary<int, ArtefactData> _artefacts = new Dictionary<int, ArtefactData>();

        internal bool IsEndorced { get; set; }

        internal float Trust
        {
            get;
            private set;
        }

        internal float? Money
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

        internal void AddMoney(float addition, Func<float> initialize)
        {
            Money = (Money ?? initialize()) + addition;
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