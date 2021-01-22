using SDKTemplate.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SDKTemplate.Commands
{
    public class RoverArmCommand : MotorCommand, ICommand
    {
        public IEnumerable<string> Keywords { get => new List<string> { "raise", "lower" }; }

        public async Task RunAsync(BoostController controller, string commandText)
        {
            await RunAsync(controller, commandText, "raise", Motors.External);
        }
    }
}


