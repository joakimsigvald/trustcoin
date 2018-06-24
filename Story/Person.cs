using System;

namespace Trustcoin.Story
{
    public interface Person : IEquatable<Person>
    {
        int Id { get; }
        string Name { get; }
        float? GetTrust(Peer person);
        float? GetMoney(Peer person);
    }
}