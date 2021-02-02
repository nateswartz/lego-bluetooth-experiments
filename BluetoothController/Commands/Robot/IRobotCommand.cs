using System.Collections.Generic;
using System.Threading.Tasks;

namespace BluetoothController.Commands.Robot
{
    public interface IRobotCommand
    {
        IEnumerable<string> Keywords { get; }
        string Description { get; }
        Task RunAsync(HubController controller, string commandText);
    }
}


