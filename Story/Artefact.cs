using System;

namespace Trustcoin.Story
{
    public class Artefact : IEquatable<Artefact>
    {
        public Artefact(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public int Id { get; }
        public string Name { get; }
        public int? OwnerId { get; set; }
        public int KnownOwnerId => OwnerId ?? throw new InvalidOperationException("Artefact has no owner");

        public bool Equals(Artefact other)
            => other != null && other.Id == Id;

        public override bool Equals(object obj)
            => Equals(obj as Artefact);

        public override int GetHashCode()
            => Id;

        public override string ToString()
            => Name;
    }
}