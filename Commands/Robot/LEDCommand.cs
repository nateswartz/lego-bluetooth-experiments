using LegoBoostController.Commands.Boost;
using LegoBoostController.Controllers;
using LegoBoostController.Models;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LegoBoostController.Commands.Robot
{
    public class LEDCommand : IRobotCommand
    {
        public IEnumerable<string> Keywords { get => new List<string> { "led" }; }

        public string Description { get => "LED(Color[Green, Blue, Red, Purple])"; }

        public async Task RunAsync(BoostController controller, string commandText)
        {
            Match m = Regex.Match(commandText, @"\((\w+)\)");
            if (m.Groups.Count == 2)
            {
                var color = m.Groups[1].Value;
                var command = new LEDBoostCommand(LEDColors.GetByName(color));
                await controller.ExecuteCommandAsync(command);
            }
        }
    }
}


