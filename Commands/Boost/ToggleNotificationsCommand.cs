namespace LegoBoostController.Commands.Boost
{
    public class ToggleNotificationsCommand : IBoostCommand
    {
        public string HexCommand { get; set; }

        public ToggleNotificationsCommand(bool notificationsEnabled, string port, string sensorMode)
        {
            var messageLength = "0a";
            var state = notificationsEnabled ? "00" : "01"; // 01 - On; 00 - Off
            HexCommand = $"{messageLength}0041{port}{sensorMode}01000000{state}";
        }
    }
}


