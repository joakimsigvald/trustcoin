using System.Collections.Generic;

namespace Trustcoin.Story
{
    internal class ArtefactData
    {
        private readonly List<Person> _endorcers = new List<Person>();

        public bool IsEndorced { get; set; }

        public bool IsEndorcedBy(Person person)
            => _endorcers.Contains(person);

        public void Endorced(Person endorcer)
        {
            _endorcers.Add(endorcer);
        }
    }
}