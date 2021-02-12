using BluetoothController.Commands.Abstract;
using BluetoothController.Controllers;
using BluetoothController.Hubs;
using BluetoothController.Models;

namespace BluetoothController.Commands.Basic
{
    public class LEDCommand : PortOutputCommandType, IPoweredUpCommand
    {
        public string HexCommand { get; set; }

        public LEDCommand(HubController controller, LEDColor color)
        {
            string port = "00";
            switch (controller.Hub.HubType)
            {
                case HubType.BoostMoveHub:
                case HubType.TwoPortHub:
                    port = "32";
                    break;
                case HubType.TwoPortHandset:
                    port = "34";
                    break;
            }
            HexCommand = AddHeader($"{port}115100{color.Code}");
        }
    }
}


