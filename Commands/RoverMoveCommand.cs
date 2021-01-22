using SDKTemplate.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SDKTemplate.Commands
{
    public class RoverMoveCommand : MotorCommand, ICommand
    {
        public IEnumerable<string> Keywords { get => new List<string> { "forward", "back" }; }

        public async Task RunAsync(BoostController controller, string commandText)
        {
            await RunAsync(controller, commandText, "forward", Motors.A_B);
        }
    }
}


