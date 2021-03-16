namespace BluetoothController
{
    public interface IBluetoothLowEnergyAdapter
    {
        bool IsScanning { get; }

        void StartBleDeviceWatcher();
        void StopBleDeviceWatcher();
    }
}