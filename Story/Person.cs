using System;

namespace Trustcoin.Story
{
    public interface Person : IEquatable<Person>
    {
        int Id { get; }
        string Name { get; }
        float? GetMoney(Peer person);
    }
}