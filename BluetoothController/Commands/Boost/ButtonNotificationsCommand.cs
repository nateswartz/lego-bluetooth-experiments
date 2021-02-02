namespace LegoBoostController.Commands.Boost
{
    public class ButtonNotificationsCommand : IBoostCommand
    {
        public string HexCommand { get; set; }

        public ButtonNotificationsCommand(bool enabled)
        {
            var messageLength = "05"; // 5 bytes
            var messageType = "01"; // Device info
            var infoType = "02"; // Button
            var action = enabled ? "02" : "03";
            HexCommand = $"{messageLength}00{messageType}{infoType}{action}";
        }
    }
}


