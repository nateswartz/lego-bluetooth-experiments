using BluetoothController.Controllers;
using LegoBoostController.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LegoBoostController.Robot
{
    public class CatSitStandCommand : MotorRobotCommand, IRobotCommand
    {
        public IEnumerable<string> Keywords { get => new List<string> { "sit", "stand" }; }

        public string Description { get => "Sit/Stand()"; }

        public async Task RunAsync(HubController controller, string commandText)
        {
            await RunAsync(controller, $"{commandText.TrimEnd(new char[2] { '(', ')' })}(20,500)", "sit", Motors.B);
        }
    }
}


