using System;
using System.Collections.Generic;
using System.Linq;

namespace Trustcoin.Story
{
    public class PrivatePerson : Person
    {
        private readonly ILogger _logger;
        private readonly Dictionary<Person, PersonData> _peers = new Dictionary<Person, PersonData>();

        public PrivatePerson(int id, string name, ILogger logger)
        {
            _logger = logger;
            Id = id;
            Name = name;
        }

        public int Id { get; }
        public string Name { get; }
        private float GeneralTrust { get; } = 0.2f;
        public float EndorcementTrustFactor { get; } = 0.1f;

        public void Endorce(Person receiver)
        {
            var receiverData = Get(receiver);
            if (receiverData.IsEndorced) return;

            EnsureTrust(receiver, receiverData);
            receiverData.Endorce();
            foreach (var peer in GetPeers(new[] {receiver}))
                peer.Endorces(this, receiver);
        }

        public float? GetTrust(Person person)
            => Get(person).Trust;

        public float? GetMoney(Person person)
            => Get(person).Money;

        public void Endorces(Person endorcer, Person receiver)
        {
            var relation = Get(endorcer).Get(receiver);
            if (relation.IsEndorcer)
                return;
            relation.IsEndorcer = true;
            relation.Strengthen();
            AddMoneyToEndorcementReceiver(endorcer, receiver, relation);
        }

        public override string ToString()
            => $"{Name}: {ShowReputation()}, {ShowMoney()}, {ShowPeers()}";

        public bool Equals(Person other)
            => other.Id == Id;

        public override bool Equals(object obj)
            => Equals(obj as Person);

        public override int GetHashCode()
            => Id;

        private PersonData Get(Person person)
        {
            if (person.Equals(this))
                throw new ArgumentException("Cannot have self as peer");
            return _peers.TryGetValue(person, out var data)
                ? data
                : (_peers[person] = new PersonData(this));
        }

        private void AddMoneyToEndorcementReceiver(Person endorcer, Person receiver, RelationData relation)
        {
            var endorcerData = Get(endorcer);
            var receiverData = Get(receiver);
            EnsureTrust(endorcer, Get(endorcer));
            EndureMoney(receiver, receiverData);
            receiverData.AddEndorcementMoney(endorcerData, relation);
        }

        private void EnsureTrust(Person receiver, PersonData receiverData)
        {
            receiverData.Trust = receiverData.Trust
                                 ?? ComputeInitialTrust(receiver);
        }

        private void EndureMoney(Person receiver, PersonData receiverData)
        {
            receiverData.Money = receiverData.Money ?? ComputeInitialMoney(receiver);
        }

        private float ComputeInitialMoney(Person receiver)
            => EstimateMoney(receiver, this) ?? 0;

        private string ShowReputation()
            => $"Reputation={EstimateReputation(this)}";

        private string ShowMoney()
            => $"Money={EstimateMoney(this)}";

        private string ShowPeers()
            => $"Peers: ({string.Join(',', GetPeers(new Person[0]).Select(p => p.Name))})";

        private float? EstimateReputation(Person target, params Person[] whosAsking)
        {
            _logger.Log(EstimationFootprint(nameof(EstimateReputation), target, whosAsking));
            return GetMedian(target, peer => peer.GetTrust(target), whosAsking);
        }

        private float ComputeInitialTrust(Person target)
            => GetMedian(target, peer => Get(peer).Trust * peer.GetTrust(target))
               ?? GeneralTrust;

        private float? EstimateMoney(Person target, params Person[] whosAsking)
        {
            _logger.Log(EstimationFootprint(nameof(EstimateMoney), target, whosAsking));
            return GetMedian(target, peer => peer.GetMoney(target), whosAsking);
        }

        private string EstimationFootprint(string method, Person target, params Person[] whosAsking)
            => Footprint(method, target.Name, whosAsking);

        private string Footprint(string method, string subject, params Person[] whosAsking)
            => $"{method}: {Name}=>{subject} ({string.Join(',', whosAsking.Select(wa => wa.Name))})";

        private float? GetMedian(Person target, Func<Person, float?> getValue, params Person[] whosAsking)
            => GetPeers(whosAsking.Concat(new[] {this, target}))
                .Select(p => GetWeightedValue(p, getValue))
                .ToList()
                .Median();

        private IEnumerable<Person> GetPeers(IEnumerable<Person> except)
            => _peers.Keys.Except(except);

        private (float, float?) GetWeightedValue(Person peer, Func<Person, float?> getValue)
            => GetWeightedValue(Get(peer).Trust, peer, getValue);

        private static (float, float?) GetWeightedValue(float? weight, Person peer, Func<Person, float?> getValue)
            => weight.HasValue
                ? (weight.Value, getValue(peer))
                : (0f, (float?) null);
    }
}