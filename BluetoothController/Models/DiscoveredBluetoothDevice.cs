namespace BluetoothController.Models
{
    public record DiscoveredBluetoothDevice
    {
        public string Name { get; set; }
        public string BluetoothDeviceId { get; set; }
    }
}
