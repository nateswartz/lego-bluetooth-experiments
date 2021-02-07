using BluetoothController.Util;

namespace BluetoothController.Commands.Boost
{
    public class ButtonNotificationsCommand : IBoostCommand
    {
        public string HexCommand { get; set; }

        public ButtonNotificationsCommand(bool enabled)
        {
            var messageType = "01"; // Device info
            var infoType = "02"; // Button
            var action = enabled ? "02" : "03";
            HexCommand = CommandHelper.AddHeader($"{messageType}{infoType}{action}");
        }
    }
}


