using System;
using System.Collections.Generic;
using System.Linq;

namespace LegoBoostController.Commands
{
    public class CatCommandFactory : ICommandFactory
    {
        private IEnumerable<ICommand> _commands =
            new List<ICommand>
            {
                // TODO: Add some more commands
                new CatMoveCommand(),
                new LEDCommand()
            };

        public ICommand GetCommand(string keyword)
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
