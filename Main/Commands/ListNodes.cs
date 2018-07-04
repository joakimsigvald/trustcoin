using System;
using Trustcoin.Story;

namespace Trustcoin.Main.Commands
{
    public class ListNodes : Command
    {
        public static readonly ListNodes Singleton = new ListNodes();

        public void Execute(params string[] args)
        {
            Program.Header2("Accounts");
            Program.TestBench.Accounts
                .ForEach(account => Console.WriteLine($"{account}"));
        }

        public string Name => "accounts";

        public char Short => 'l';
    }
}