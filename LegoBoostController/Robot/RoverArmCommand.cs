using BluetoothController.Controllers;
using LegoBoostController.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LegoBoostController.Robot
{
    public class RoverArmCommand : MotorRobotCommand, IRobotCommand
    {
        public IEnumerable<string> Keywords { get => new List<string> { "raise", "lower" }; }

        public string Description { get => "Raise/Lower(Speed, Time)"; }

        public async Task RunAsync(IHubController controller, string commandText)
        {
            await RunAsync(controller, commandText, "raise", Motors.External);
        }
    }
}


