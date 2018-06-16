namespace Trustcoin.Main.Commands
{
    public class SmartEndorce : SmartCommand
    {
        public Command[] Commands => new Command[] { new Endorce(), new ListNodes() };
        public char Short => '*';
    }
}