using BluetoothController.Util;

namespace BluetoothController.Commands.Boost
{
    public class HubNameCommand : IBoostCommand
    {
        public string HexCommand { get; set; }

        public HubNameCommand()
        {
            var messageType = "01"; // Device info
            var infoType = "01"; // Name
            var action = "05"; // One-time request
            HexCommand = CommandHelper.AddHeader($"{messageType}{infoType}{action}01");
        }
    }
}


