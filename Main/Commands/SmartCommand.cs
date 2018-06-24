namespace Trustcoin.Main.Commands
{
    public class SmartCommand
    {
        private readonly Command _primaryCommand;
        private readonly Command[] _postCommands;

        public SmartCommand(Command primaryCommand, params Command[] postCommands)
        {
            _primaryCommand = primaryCommand;
            _postCommands = postCommands;
        }

        public CommandMatch Match(string input)
            => _primaryCommand.Short == input[0]
                ? new CommandMatch
                {
                    Arguments = input.Substring(1).Split(','),
                    PrimaryCommand = _primaryCommand,
                    PostCommands = _postCommands
                }
                : null;
    }
}