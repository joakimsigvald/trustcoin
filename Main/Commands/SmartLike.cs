namespace Trustcoin.Main.Commands
{
    public class SmartLike : SmartCommand
    {
        public Command[] Commands => new Command[] { new Like(), new ListNodes() };
        public char Short => '*';
    }
}