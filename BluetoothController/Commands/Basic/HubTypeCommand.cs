using BluetoothController.Commands.Abstract;

namespace BluetoothController.Commands.Basic
{
    public class HubTypeCommand : DeviceInfoCommandType, ICommand
    {
        public string HexCommand { get; set; }

        public HubTypeCommand()
        {
            var infoType = "0B"; // Device Type
            var action = "05"; // One-time request
            HexCommand = AddHeader($"{infoType}{action}00");
        }
    }
}


