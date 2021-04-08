namespace BluetoothController.Models
{
    public record DiscoveredDevice
    {
        public string Name { get; set; }
        public string BluetoothDeviceId { get; set; }
    }
}
