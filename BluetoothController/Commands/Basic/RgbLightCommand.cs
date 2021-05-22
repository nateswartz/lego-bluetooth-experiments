using BluetoothController.Commands.Abstract;
using BluetoothController.Controllers;
using BluetoothController.Models;
using System.Linq;

namespace BluetoothController.Commands.Basic
{
    public class RgbLightCommand : PortOutputCommandType
    {
        public RgbLightCommand(IHubController controller, RgbLightColor color)
        {
            var port = controller.Hub.GetPortsByDeviceType(IOTypes.RgbLight).First().PortID;
            HexCommand = AddHeader($"{port}115100{color.Code}");
        }
    }
}
