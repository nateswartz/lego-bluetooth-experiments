namespace BluetoothController.Commands.Boost
{
    public class HubFirmwareCommand : DeviceInfoCommand, IBoostCommand
    {
        public string HexCommand { get; set; }

        public HubFirmwareCommand()
        {
            var infoType = "03"; // Firmware
            var action = "05"; // One-time request
            HexCommand = AddHeader($"{infoType}{action}00");
        }
    }
}


