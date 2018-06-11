namespace Trustcoin.Main.Commands
{
    public interface SmartCommand
    {
        Command[] Commands { get; }
        char Short { get; }
    }
}