using BluetoothController.Models;

namespace BluetoothController.Hubs
{
    public class HubPort
    {
        public string PortID { get; set; }
        public IoDeviceType DeviceType { get; set; }
        public string NotificationMode { get; set; }
    }
}
