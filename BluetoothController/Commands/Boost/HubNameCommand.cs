namespace BluetoothController.Commands.Boost
{
    public class HubNameCommand : DeviceInfoCommand, IBoostCommand
    {
        public string HexCommand { get; set; }

        public HubNameCommand()
        {
            var infoType = "01"; // Name
            var action = "05"; // One-time request
            HexCommand = AddHeader($"{infoType}{action}01");
        }
    }
}


