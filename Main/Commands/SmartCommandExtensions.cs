using System.Collections.Generic;
using System.Linq;

namespace Trustcoin.Main.Commands
{
    public static class SmartCommandExtensions
    {
        public static Command PrimaryCommand(this SmartCommand sc)
            => sc.Commands[0];

        public static IEnumerable<Command> PostCommands(this SmartCommand sc)
            => sc.Commands.Skip(1);

        public static char Short(this SmartCommand sc)
            => sc.PrimaryCommand().Short;
    }
}
