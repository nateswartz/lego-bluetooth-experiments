using BluetoothController.Controllers;
using BluetoothController.Models;
using BluetoothController.Util;

namespace BluetoothController.Commands.Boost
{
    public class LEDBoostCommand : IBoostCommand
    {
        public string HexCommand { get; set; }

        public LEDBoostCommand(HubController controller, LEDColor color)
        {
            string port = "00";
            switch (controller.HubType)
            {
                case HubType.BoostMoveHub:
                case HubType.TwoPortHub:
                    port = "32";
                    break;
                case HubType.TwoPortHandset:
                    port = "34";
                    break;
            }
            HexCommand = CommandHelper.AddHeader($"81{port}115100{color.Code}");
        }
    }
}


