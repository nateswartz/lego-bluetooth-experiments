using System;
using System.Collections.Generic;
using System.Linq;

namespace SDKTemplate.Commands
{
    public static class CommandFactory
    {
        private static IEnumerable<ICommand> _commands =
            new List<ICommand>
            {
                new MoveCommand(),
                new SpinCommand(),
                new ArmCommand(),
                new LEDCommand()
            };

        public static ICommand GetCommand(string keyword)
        {
            foreach (var command in _commands)
            {
                if (command.Keywords.Any(k => k == keyword))
                {
                    return command;
                }
            }
            throw new Exception($"No command matching keyword: {keyword}");
        }
    }
}
