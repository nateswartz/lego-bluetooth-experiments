using BluetoothController.Controllers;
using BluetoothController.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BluetoothController.Commands.Robot
{
    public class RoverMoveCommand : MotorCommand, IRobotCommand
    {
        public IEnumerable<string> Keywords { get => new List<string> { "forward", "back" }; }

        public string Description { get => "Forward/Back(Speed, Time)"; }

        public async Task RunAsync(HubController controller, string commandText)
        {
            await RunAsync(controller, commandText, "forward", Motors.A_B);
        }
    }
}


