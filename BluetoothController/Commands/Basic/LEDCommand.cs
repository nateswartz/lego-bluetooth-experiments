using BluetoothController.Commands.Abstract;
using BluetoothController.Controllers;
using BluetoothController.Models;
using System.Linq;

namespace BluetoothController.Commands.Basic
{
    public class LEDCommand : PortOutputCommandType
    {
        public LEDCommand(IHubController controller, LEDColor color)
        {
            var port = controller.Hub.GetPortsByDeviceType(IOTypes.LED).First().PortID;
            HexCommand = AddHeader($"{port}115100{color.Code}");
        }
    }
}
