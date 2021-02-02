namespace BluetoothController.Commands.Boost
{
    public class HubTypeCommand : IBoostCommand
    {
        public string HexCommand { get; set; }

        public HubTypeCommand()
        {
            var messageLength = "06"; // 6 bytes
            var messageType = "01"; // Device info
            var infoType = "0B"; // Device Type
            var action = "05"; // One-time request
            HexCommand = $"{messageLength}00{messageType}{infoType}{action}00";
        }
    }
}


