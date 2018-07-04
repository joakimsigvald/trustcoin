using System.Collections.Generic;

namespace Trustcoin.Story
{
    internal class ArtefactData
    {
        private readonly List<int> _complimenters = new List<int>();

        public ArtefactData(Artefact artefact)
            => Name = artefact.Name;

        public string Name { get; }
        public bool IsEndorced { get; set; }

        public bool IsComplementedBy(int person)
            => _complimenters.Contains(person);

        public void Complimented(int endorcer)
        {
            _complimenters.Add(endorcer);
        }

        public override string ToString()
            => Name;
    }
}