using BluetoothController.Commands.Abstract;

namespace BluetoothController.Commands.Basic
{
    public class ButtonNotificationsCommand : DeviceInfoCommandType
    {
        public ButtonNotificationsCommand(bool enabled)
        {
            var infoType = "02"; // Button
            var action = enabled ? "02" : "03";
            HexCommand = AddHeader($"{infoType}{action}");
        }
    }
}


