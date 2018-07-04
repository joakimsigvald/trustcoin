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

        private static readonly SmartCommand[] SmartCommands =
            CreateSmartCommands(typeof(Quit), typeof(ListNodes))
            .ToArray();

        private static IEnumerable<SmartCommand> CreateSmartCommands(params Type[] excludedCommandTypes)
            => Commands.Where(cmd => !excludedCommandTypes.Contains(cmd.GetType()))
                .Select(cmd => new SmartCommand(cmd, ListNodes.Singleton));

        public static readonly TestBench TestBench = new TestBench(new FactoryImpl(new InstantNetwork()));

        static void Main(string[] args)
        {
            Header1("Trustcoin");
            foreach (var commands in args.Select(GetCommands))
                RunCommands(commands);
            while (true)
                RunCommands(ReadCommands());
        }

        private static void RunCommands(CommandMatch match)
        {
            RunCommand(match.PrimaryCommand, match.Arguments);
            foreach (var command in match.PostCommands)
            {
                RunCommand(command);
            }
        }

        private static void RunCommand(Command command, params string[] arguments)
        {
            Output(command, arguments);
            command.Execute(arguments);
        }

        private static void Output(Command command, params string[] arguments)
        {
            Output();
            Output($"{command.Name}: {string.Join(',', arguments)}/");
        }

        private static void Output(string line = null)
        {
            Console.WriteLine(line);
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
            => GetCommands(Ask("Write command"));

        private static CommandMatch GetCommands(string input) 
            => CommandMatch.Create(Commands.FirstOrDefault(cmd => cmd.Matches(input)))
                                                                 ?? SmartCommands
                                                                     .Select(sc => sc.Match(input))
                                                                     .FirstOrDefault(sc => sc != null)
                                                                 ?? CommandMatch.Create(new ListCommands());

        private static TParent[] FindAll<TParent>()
            => GetTypesInNamespace<TParent>(Assembly.GetExecutingAssembly())
                .Select(t => (TParent)Activator.CreateInstance(t))
                .ToArray();

        private static IEnumerable<Type> GetTypesInNamespace<TParent>(Assembly assembly)
            => assembly.GetTypes()
                .Where(t => typeof(TParent).IsAssignableFrom(t) && t.IsClass);
    }
}