using LegoBoostController.Controllers;
using LegoBoostController.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LegoBoostController.Commands
{
    public class RoverArmCommand : MotorCommand, ICommand
    {
        public IEnumerable<string> Keywords { get => new List<string> { "raise", "lower" }; }

        public string Description { get => "Raise/Lower(Speed, Time)"; }

        public async Task RunAsync(BoostController controller, string commandText)
        {
            await RunAsync(controller, commandText, "raise", Motors.External);
        }
    }
}


