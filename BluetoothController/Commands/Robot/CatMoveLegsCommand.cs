using BluetoothController.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BluetoothController.Commands.Robot
{
    public class CatMoveLegsCommand : MotorCommand, IRobotCommand
    {
        public IEnumerable<string> Keywords { get => new List<string> { "up", "down" }; }

        public string Description { get => "Up/Down(Speed, Time)"; }

        public async Task RunAsync(HubController controller, string commandText)
        {
            await RunAsync(controller, commandText, "down", Motors.B);
        }
    }
}


