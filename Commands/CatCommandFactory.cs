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
                new LEDCommand(),
                new CatMoveLegsCommand(),
                new CatSitStandCommand(),
                new CatMoveTailCommand(),
                new CatMoveEyesCommand()
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
            var examples = $"Cat:";
            foreach (var command in _commands)
            {
                examples += $"{Environment.NewLine}{command.Description}";
            }
            return examples;
        }
    }
}
