using System;
using System.Collections.Generic;
using System.Linq;

namespace LegoBoostController.Commands
{
    public class RoverCommandFactory : ICommandFactory
    {
        private IEnumerable<ICommand> _commands =
            new List<ICommand>
            {
                new RoverMoveCommand(),
                new RoverSpinCommand(),
                new RoverArmCommand(),
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

        public string GetCommandExamples()
        {
            var examples = $"Rover:";
            foreach (var command in _commands)
            {
                examples += $"{Environment.NewLine}{command.Description}";
            }
            return examples;
        }
    }
}
