namespace BluetoothController.Commands.Boost
{
    public class HubTypeCommand : DeviceInfoCommand, IBoostCommand
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


