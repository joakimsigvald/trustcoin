using System.Collections.Generic;

namespace Trustcoin.Story
{
    public class PersonData
    {
        private const float MaxTrust = 0.999f;

        private readonly Person _subject;

        public PersonData(Person subject)
        {
            _subject = subject;
        }

        private bool IsEndorced { get; set; }
        private readonly Dictionary<Person, RelationData> _relations = new Dictionary<Person, RelationData>();

        public float? Trust
        {
            get; set;
        }

        public float? Money
        {
            get; set;
        }

        public void Endorce()
        {
            if (IsEndorced) return;
            IsEndorced = true;
            IncreaseTrust();
        }

        private void IncreaseTrust()
        {
            Trust += _subject.EndorcementTrustFactor * (MaxTrust - Trust);
        }

        public RelationData Get(Person person)
            => _relations.TryGetValue(person, out var nd)
                ? nd
                : _relations[person] = new RelationData();
    }
}