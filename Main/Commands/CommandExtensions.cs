namespace Trustcoin.Main.Commands
{
    public static class CommandExtensions
    {
        public static bool Matches(this Command cmd, string input)
            => $"{cmd.Short}" == input || cmd.Name == input;

        public static string Signature(this Command cmd)
            => $"{cmd.Short}={cmd.Name}";
    }
}