using BluetoothController.Util;

namespace BluetoothController.Commands.Boost
{
    public class HubFirmwareCommand : IBoostCommand
    {
        public string HexCommand { get; set; }

        public HubFirmwareCommand()
        {
            var messageType = "01"; // Device info
            var infoType = "03"; // Firmware
            var action = "05"; // One-time request
            HexCommand = CommandHelper.AddHeader($"{messageType}{infoType}{action}00");
        }
    }
}


