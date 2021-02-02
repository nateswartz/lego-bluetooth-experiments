namespace BluetoothController.Commands.Boost
{
    public class HubVoltageNotificationsCommand : IBoostCommand
    {
        public string HexCommand { get; set; }

        public HubVoltageNotificationsCommand(bool enabled)
        {
            var messageLength = "0A"; // 10 bytes
            var messageType = "41"; // Port Input Format Setup (Single)
            var portID = "3C"; // Voltage
            var interval = "05000000"; // Delta Interval
            var notificationEnabled = enabled ? "01" : "00"; // Enabled
            HexCommand = $"{messageLength}00{messageType}{portID}00{interval}{notificationEnabled}";
        }
    }
}


