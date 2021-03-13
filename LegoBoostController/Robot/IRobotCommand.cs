using BluetoothController.Controllers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LegoBoostController.Robot
{
    public interface IRobotCommand
    {
        IEnumerable<string> Keywords { get; }
        string Description { get; }
        Task RunAsync(IHubController controller, string commandText);
    }
}


