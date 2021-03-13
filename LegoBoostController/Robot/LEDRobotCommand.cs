using BluetoothController.Commands.Basic;
using BluetoothController.Controllers;
using BluetoothController.Models;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LegoBoostController.Robot
{
    public class LEDRobotCommand : IRobotCommand
    {
        public IEnumerable<string> Keywords { get => new List<string> { "led" }; }

        public string Description { get => "LED(Color[Green, Blue, Red, Purple])"; }

        public async Task RunAsync(IHubController controller, string commandText)
        {
            Match m = Regex.Match(commandText, @"\((\w+)\)");
            if (m.Groups.Count == 2)
            {
                var color = m.Groups[1].Value;
                var command = new LEDCommand(controller, LEDColors.GetByName(color));
                await controller.ExecuteCommandAsync(command);
            }
        }
    }
}


