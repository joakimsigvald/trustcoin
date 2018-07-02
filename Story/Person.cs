using System;

namespace Trustcoin.Story
{
    public interface Person : IEquatable<Person>
    {
        int Id { get; }
        string Name { get; }
        ConfidenceValue GetMoney(Peer person, Guid? beforeTransaction, params Peer[] whosAsking);
    }
}