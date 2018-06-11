namespace Trustcoin.Main.Commands
{
    public interface Command
    {
        void Execute(params string[] args);
        string Name { get; }
        char Short { get; }
    }
}