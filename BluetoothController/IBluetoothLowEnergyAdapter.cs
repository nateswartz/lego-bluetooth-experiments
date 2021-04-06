namespace BluetoothController
{
    public interface IBluetoothLowEnergyAdapter
    {
        void StartBleDeviceWatcher();
        void StopBleDeviceWatcher();
    }
}