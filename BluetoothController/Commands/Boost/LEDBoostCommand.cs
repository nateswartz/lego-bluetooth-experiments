using BluetoothController.Models;
using BluetoothController.Util;

namespace BluetoothController.Commands.Boost
{
    public class LEDBoostCommand : IBoostCommand
    {
        public string HexCommand { get; set; }

        public LEDBoostCommand(LEDColor color)
        {
            // TODO: For remote, port is 34, not 32
            var port = "32";
            HexCommand = CommandHelper.AddHeader($"81{port}115100{color.Code}");
        }
    }
}


