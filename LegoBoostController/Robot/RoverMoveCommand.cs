using BluetoothController.Controllers;
using LegoBoostController.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LegoBoostController.Robot
{
    public class RoverMoveCommand : MotorRobotCommand, IRobotCommand
    {
        public IEnumerable<string> Keywords { get => new List<string> { "forward", "back" }; }

        public string Description { get => "Forward/Back(Speed, Time)"; }

        public async Task RunAsync(HubController controller, string commandText)
        {
            await RunAsync(controller, commandText, "forward", Motors.A_B);
        }
    }
}


