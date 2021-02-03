using BluetoothController.Controllers;
using BluetoothController.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BluetoothController.Commands.Robot
{
    public class RoverArmCommand : MotorCommand, IRobotCommand
    {
        public IEnumerable<string> Keywords { get => new List<string> { "raise", "lower" }; }

        public string Description { get => "Raise/Lower(Speed, Time)"; }

        public async Task RunAsync(HubController controller, string commandText)
        {
            await RunAsync(controller, commandText, "raise", Motors.External);
        }
    }
}


