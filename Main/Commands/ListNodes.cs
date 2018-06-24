using System;
using Trustcoin.Story;

namespace Trustcoin.Main.Commands
{
    public class ListNodes : Command
    {
        public void Execute(params string[] args)
        {
            Program.Header2("Participants");
            Program.Network.Accounts
                .ForEach(account => Console.WriteLine($"{account}"));
        }

        public string Name => "accounts";

        public char Short => 'l';
    }
}