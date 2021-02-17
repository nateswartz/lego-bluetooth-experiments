using BluetoothController.Commands.Abstract;

namespace BluetoothController.Commands.Basic
{
    public class ToggleNotificationsCommand : PortInputFormatSetupSingleCommandType, IPoweredUpCommand
    {
        public string HexCommand { get; set; }

        // TODO: Make sensor mode more user friendly
        // For Tilt Sensor, 01-04 all work, vary granularity
        // For Internal Motor (single/combined), 01-02 work, 03-04 don't
        // For External Motor, 02 is angle data, 01 is speed data, 03 is more granular something, 04 doesn't work
        // For Color Distance
        //      00 is just color, 01 is just distance, 02 seems to send a value which increments every time something gets close
        //      03 is distance, increasing as something is closer,
        //      08 is color and distance
        public ToggleNotificationsCommand(string portId, bool enableNotifications, string sensorMode)
        {
            var state = enableNotifications ? "01" : "00"; // 01 - On; 00 - Off
            var deltaInterval = "01000000"; // value must change by this interval to trigger a message, protects against jitter
            HexCommand = AddHeader($"{portId}{sensorMode}{deltaInterval}{state}");
        }
    }
}


