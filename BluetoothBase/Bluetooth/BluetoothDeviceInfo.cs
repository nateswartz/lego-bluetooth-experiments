namespace BluetoothBase.Bluetooth
{
    public sealed class BluetoothDeviceInfo
    {
        public ulong BluetoothAddress { get; set; }
        public byte[] ManufacturerData { get; set; }
        public string Name { get; set; }
    }
}