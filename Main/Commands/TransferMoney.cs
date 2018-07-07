using Trustcoin.Story;

namespace Trustcoin.Main.Commands
{
    public class TransferMoney : Command
    {
        public void Execute(params string[] args)
        {
            var senderName = args.NthOrDefault(0) ?? Program.Ask("Name sender");
            var receiverName = args.NthOrDefault(1) ?? Program.Ask("Name receiver");
            var amount = args.NthOrDefault(2) ?? Program.Ask("Specify amount");
            Program.TestBench.TransferMoney(senderName, receiverName, float.Parse(amount));
        }

        public string Name => "transfer";

        public char Short => '$';
    }
}