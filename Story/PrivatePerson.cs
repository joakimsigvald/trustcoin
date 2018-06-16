using System;
using System.Collections.Generic;
using System.Linq;

namespace Trustcoin.Story
{
    public class PrivatePerson : Person, IEquatable<Person>
    {
        private readonly Dictionary<Person, PersonData> _peers = new Dictionary<Person, PersonData>();

        public PrivatePerson(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public int Id { get; }
        public string Name { get; }
        public float GeneralTrust { get; set; } = 0.2f;
        public float EndorcementTrustFactor { get; set; } = 0.1f;

        public void Endorce(Person receiver)
        {
            var receiverData = Get(receiver);
            receiverData.Endorce();
            var generatedEndorcementMoney =
                GetMedian(receiver, peer => peer.Endorces(this, receiver)) ?? 1f;
            AddMoney(receiver, generatedEndorcementMoney);
        }

        public PersonData Get(Person person)
            => _peers.TryGetValue(person, out var data)
                ? data
                : _peers[person] = new PersonData(this);

        public float? GetTrust(Person person)
            => _peers.TryGetValue(person, out var data)
                ? data.Trust ?? GeneralTrust
                : GeneralTrust;

        public float? GetMoney(Person person)
            => _peers.TryGetValue(person, out var data)
                ? data.Money
                : null;

        public float Endorces(Person endorcer, Person receiver)
        {
            var relation = Get(endorcer).Get(receiver);
            if (relation.IsEndorcer)
                return 0;
            relation.IsEndorcer = true;
            var endorcementMoney = GenerateEndorcementMoney(endorcer, relation);
            relation.Strengthen();
            AddMoney(receiver, endorcementMoney);
            return endorcementMoney;
        }

        private void AddMoney(Person receiver, float newMoney)
        {
            var receiverData = Get(receiver);
            if (!receiverData.Money.HasValue)
                receiverData.Money = EstimateMoney(receiver) ?? 0;
            receiverData.Money += newMoney;
        }

        public override string ToString()
            => $"{Name}: Reputation={EstimateReputation(this)}, Money={EstimateMoney(this)}, Peers: ({string.Join(',', GetPeers().Select(p => p.Name))})";

        public bool Equals(Person other)
            => other.Id == Id;

        public override bool Equals(object obj)
            => Equals(obj as Person);

        public override int GetHashCode()
            => Id;

        private float GenerateEndorcementMoney(Person endorcer, RelationData relation)
            => (float)Math.Sqrt((GetTrust(endorcer) ?? GeneralTrust) * (1 - relation.Strength));

        private float? EstimateReputation(Person target)
            => GetMedian(target, peer => peer.GetTrust(target));

        private float? EstimateMoney(Person target)
            => GetMedian(target, peer => peer.GetMoney(target));

        private float? GetMedian(Person target, Func<Person, float?> getValue)
            => GetPeers(except: target)
                .Select(p => GetWeightedValue(p, target, getValue))
                .ToList()
                .Median();

        private IEnumerable<Person> GetPeers(params Person[] except)
            => _peers.Keys.Except(except);

        private (float, float?) GetWeightedValue(Person peer, Person target, Func<Person, float?> getValue)
            => (GetTrust(peer) ?? GeneralTrust, getValue(target));
    }
}