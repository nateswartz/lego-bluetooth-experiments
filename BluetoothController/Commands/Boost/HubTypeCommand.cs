using BluetoothController.Util;

namespace BluetoothController.Commands.Boost
{
    public class HubTypeCommand : IBoostCommand
    {
        public string HexCommand { get; set; }

        public HubTypeCommand()
        {
            var messageType = "01"; // Device info
            var infoType = "0B"; // Device Type
            var action = "05"; // One-time request
            HexCommand = CommandHelper.AddHeader($"{messageType}{infoType}{action}00");
        }
    }
}


