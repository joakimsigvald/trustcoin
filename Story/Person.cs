using System.Collections.Generic;

namespace Trustcoin.Story
{
    public interface Person
    {
        int Id { get; }
        string Name { get; }
        float GeneralTrust { get; set; }
        float EndorcementTrustFactor { get; set; }
        void Endorce(Person person);
        float Endorces(Person endorcer, Person receiver);
        PersonData Get(Person person);
        float? GetTrust(Person person);
        float? GetMoney(Person person);
    }
}