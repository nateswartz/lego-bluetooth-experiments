namespace BluetoothController.Commands.Boost
{
    public class HubFirmwareCommand : IBoostCommand
    {
        public string HexCommand { get; set; }

        public HubFirmwareCommand()
        {
            var messageLength = "06"; // 6 bytes
            var messageType = "01"; // Device info
            var infoType = "03"; // Firmware
            var action = "05"; // One-time request
            HexCommand = $"{messageLength}00{messageType}{infoType}{action}00";
        }
    }
}


