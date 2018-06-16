using System.Linq;

namespace Trustcoin.Main.Commands
{
    public static class CommandExtensions
    {
        public static bool Matches(this Command cmd, string input)
            => $"{cmd.Short}" == input || cmd.Name == input;

        public static string Signature(this Command cmd)
            => $"{cmd.Short}={cmd.Name}";

        public static CommandMatch Match(this SmartCommand cmd, string input)
            => cmd.Short == input[0]
                ? new CommandMatch
                {
                    Arguments = input.Substring(1).Split(','),
                    PrimaryCommand = cmd.Commands[0],
                    PostCommands = cmd.Commands.Skip(1)
                }
                : null;
    }
}