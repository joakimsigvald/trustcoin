﻿namespace Trustcoin.Main.Commands
{
    public class SmartAddNode : SmartCommand
    {
        public Command[] Commands => new Command[] { new AddNode(), new ListNodes() };
        public char Short => '+';
    }
}