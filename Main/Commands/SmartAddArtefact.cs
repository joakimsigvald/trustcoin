namespace Trustcoin.Main.Commands
{
    public class SmartAddArtefact : SmartCommand
    {
        public Command[] Commands => new Command[] { new AddArtefact(), new ListNodes() };
    }
}