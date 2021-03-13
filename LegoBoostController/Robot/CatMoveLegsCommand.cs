using BluetoothController.Controllers;
using LegoBoostController.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LegoBoostController.Robot
{
    public class CatMoveLegsCommand : MotorRobotCommand, IRobotCommand
    {
        public IEnumerable<string> Keywords { get => new List<string> { "up", "down" }; }

        public string Description { get => "Up/Down(Speed, Time)"; }

        public async Task RunAsync(IHubController controller, string commandText)
        {
            await RunAsync(controller, commandText, "down", Motors.B);
        }
    }
}


