using BluetoothController.Models;

namespace BluetoothController.Hubs
{
    internal class HubPort
    {
        public string PortID { get; set; }
        public IOType DeviceType { get; set; }
        public string NotificationMode { get; set; }
    }
}
