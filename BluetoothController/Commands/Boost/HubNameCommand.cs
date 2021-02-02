namespace LegoBoostController.Commands.Boost
{
    public class HubNameCommand : IBoostCommand
    {
        public string HexCommand { get; set; }

        public HubNameCommand()
        {
            var messageLength = "06"; // 6 bytes
            var messageType = "01"; // Device info
            var infoType = "01"; // Name
            var action = "05"; // One-time request
            HexCommand = $"{messageLength}00{messageType}{infoType}{action}00";
        }
    }
}


