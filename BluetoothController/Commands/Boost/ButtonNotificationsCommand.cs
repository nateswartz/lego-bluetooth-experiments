using BluetoothController.Models;
using BluetoothController.Util;

namespace BluetoothController.Commands.Boost
{
    public class ButtonNotificationsCommand : DeviceInfoCommand, IBoostCommand
    {
        public string HexCommand { get; set; }

        public ButtonNotificationsCommand(bool enabled)
        {
            var infoType = "02"; // Button
            var action = enabled ? "02" : "03";
            HexCommand = AddHeader($"{infoType}{action}");
        }
    }

    public abstract class DeviceInfoCommand
    {
        public string MessageType { get; set; } = CommandTypes.DeviceInfo;

        public string AddHeader(string command)
        {
            return CommandHelper.AddHeader($"{MessageType}{command}");
        }
    }

    public abstract class HubActionCommand
    {
        public string MessageType { get; set; } = CommandTypes.HubAction;

        public string AddHeader(string command)
        {
            return CommandHelper.AddHeader($"{MessageType}{command}");
        }
    }
}


