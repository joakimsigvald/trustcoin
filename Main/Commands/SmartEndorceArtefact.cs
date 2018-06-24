namespace Trustcoin.Main.Commands
{
    public class SmartEndorceArtefact : SmartCommand
    {
        public Command[] Commands => new Command[] { new EndorceArtefact(), new ListNodes() };
    }
}