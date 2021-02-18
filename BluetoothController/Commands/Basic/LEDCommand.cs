using BluetoothController.Commands.Abstract;
using BluetoothController.Controllers;
using BluetoothController.Models;
using System.Linq;

namespace BluetoothController.Commands.Basic
{
    public class LEDCommand : PortOutputCommandType, IPoweredUpCommand
    {
        public string HexCommand { get; set; }

        public LEDCommand(HubController controller, LEDColor color)
        {
            var port = controller.Hub.GetPortsByDeviceType(IOType.LED).First().PortID;
            HexCommand = AddHeader($"{port}115100{color.Code}");
        }
    }
}


