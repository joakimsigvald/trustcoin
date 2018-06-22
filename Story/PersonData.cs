using System;
using System.Collections.Generic;

namespace Trustcoin.Story
{
    internal class PersonData
    {
        private const float MaxTrust = 0.999f;

        private readonly Person _subject;
        private readonly Dictionary<Person, RelationData> _relations = new Dictionary<Person, RelationData>();

        internal PersonData(Person subject)
        {
            _subject = subject;
        }

        internal bool IsEndorced { get; private set; }

        internal float? Trust
        {
            get; set;
        }

        internal float? Money
        {
            get; set;
        }

        internal void Endorce()
        {
            IsEndorced = true;
            Money = null;
            IncreaseTrust();
        }

        internal RelationData Get(Person person)
            => _relations.TryGetValue(person, out var nd)
                ? nd
                : _relations[person] = new RelationData();

        internal void AddEndorcementMoney(PersonData endorcerData, RelationData relation)
        {
            Money += endorcerData.GenerateEndorcementMoney(relation);
        }

        private void IncreaseTrust()
        {
            Trust += _subject.EndorcementTrustFactor * (MaxTrust - Trust);
        }

        private float GenerateEndorcementMoney(RelationData relation)
            => (float)Math.Sqrt(Trust.Value * (1 - relation.Strength));
    }
}