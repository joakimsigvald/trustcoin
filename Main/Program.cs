using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Trustcoin.Main.Commands;
using Trustcoin.Story;

namespace Trustcoin.Main
{
    public class Program
    {
        public static readonly Command[] Commands = FindAll<Command>();
        private static readonly SmartCommand[] SmartCommands = FindAll<SmartCommand>();

        public static readonly Network Network = new Network();

        static void Main(string[] args)
        {
            Header1("Trustcoin");
            while (true)
            {
                var match = ReadCommands();
                match.PrimaryCommand.Execute(match.Arguments);
                foreach (var command in match.PostCommands)
                {
                    command.Execute();
                }
            }
        }

        internal static void Header2(string caption)
        {
            WriteHeader(caption, '-');
        }

        internal static string Ask(string question)
        {
            Console.WriteLine();
            Console.WriteLine($"{question}:");
            return Console.ReadLine();
        }

        private static void Header1(string caption)
        {
            WriteHeader(caption, '=');
        }

        private static void WriteHeader(string caption, char underlineChar)
        {
            Console.WriteLine();
            Console.WriteLine(caption);
            Console.WriteLine(new string(underlineChar, caption.Length));
        }

        private static CommandMatch ReadCommands()
        {
            var input = Ask("Write command");
            return CommandMatch.Create(Commands.FirstOrDefault(cmd => cmd.Matches(input)))
                ?? SmartCommands
                .Select(sc => sc.Match(input))
                .FirstOrDefault(sc => sc != null)
            ?? CommandMatch.Create(new ListCommands());
        }

        private static TParent[] FindAll<TParent>()
            => GetTypesInNamespace<TParent>(Assembly.GetExecutingAssembly())
                .Select(t => (TParent)Activator.CreateInstance(t))
                .ToArray();

        private static IEnumerable<Type> GetTypesInNamespace<TParent>(Assembly assembly)
            => assembly.GetTypes()
                .Where(t => typeof(TParent).IsAssignableFrom(t) && t.IsClass);
    }
}