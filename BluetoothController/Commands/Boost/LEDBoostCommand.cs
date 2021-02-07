using BluetoothController.Models;
using BluetoothController.Util;

namespace BluetoothController.Commands.Boost
{
    public class LEDBoostCommand : IBoostCommand
    {
        public string HexCommand { get; set; }

        public LEDBoostCommand(LEDColor color)
        {
            HexCommand = CommandHelper.AddHeader($"8132115100{color.Code}");
        }
    }
}


