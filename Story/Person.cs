using System;

namespace Trustcoin.Story
{
    public interface Person : IEquatable<Person>
    {
        int Id { get; }
        string Name { get; }
        float EndorcementTrustFactor { get; }
        void Endorce(Person person);
        void Endorces(Person endorcer, Person receiver);
        float? GetTrust(Person person);
        float? GetMoney(Person person);
    }
}