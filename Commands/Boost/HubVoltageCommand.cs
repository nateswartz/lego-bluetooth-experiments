namespace LegoBoostController.Commands.Boost
{
    public class HubVoltageCommand : IBoostCommand
    {
        public string HexCommand { get; set; }

        public HubVoltageCommand()
        {
            var messageLength = "06"; // 6 bytes
            var messageType = "01"; // Device info
            var infoType = "06"; // Voltage
            var action = "05"; // One-time request
            HexCommand = $"{messageLength}00{messageType}{infoType}{action}00";
        }
    }
}


