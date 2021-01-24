using LegoBoostController.Controllers;
using LegoBoostController.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LegoBoostController.Commands
{
    public class RoverMoveCommand : MotorCommand, ICommand
    {
        public IEnumerable<string> Keywords { get => new List<string> { "forward", "back" }; }

        public string Description { get => "Forward/Back(Speed, Time)"; }

        public async Task RunAsync(BoostController controller, string commandText)
        {
            await RunAsync(controller, commandText, "forward", Motors.A_B);
        }
    }
}


