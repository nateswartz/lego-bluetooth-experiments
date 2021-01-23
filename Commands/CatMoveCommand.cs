using LegoBoostController.Controllers;
using LegoBoostController.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LegoBoostController.Commands
{
    public class CatMoveCommand : MotorCommand, ICommand
    {
        public IEnumerable<string> Keywords { get => new List<string> { "up", "down" }; }

        public async Task RunAsync(BoostController controller, string commandText)
        {
            await RunAsync(controller, commandText, "up", Motors.B);
        }
    }
}


