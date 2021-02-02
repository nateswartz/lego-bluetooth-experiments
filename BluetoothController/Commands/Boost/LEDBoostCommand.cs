using BluetoothController.Models;

namespace BluetoothController.Commands.Boost
{
    public class LEDBoostCommand : IBoostCommand
    {
        public string HexCommand { get; set; }

        public LEDBoostCommand(LEDColor color)
        {
            HexCommand = $"08008132115100{color.Code}";
        }
    }
}


