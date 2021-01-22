using SDKTemplate.Commands;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SDKTemplate
{
    public class TextCommandsController
    {
        private readonly BoostController _controller;

        public TextCommandsController(BoostController controller)
        {
            _controller = controller;
        }

        public async Task RunCommandsAsync(string commands)
        {
            if (!String.IsNullOrEmpty(commands))
            {
                var statements = commands.Split(';').Where(c => !string.IsNullOrEmpty(c));
                foreach (var statement in statements)
                {
                    var commandToRun = Regex.Replace(statement.ToLower(), @"\s+", "");
                    var keyword = commandToRun.Split('(')[0];
                    var command = CommandFactory.GetCommand(keyword);
                    await command.RunAsync(_controller, commandToRun);
                    await Task.Delay(500);
                }
            }
        }
    }
}
