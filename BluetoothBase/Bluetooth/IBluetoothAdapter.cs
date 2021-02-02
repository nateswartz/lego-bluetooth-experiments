using System;
using System.Threading;
using System.Threading.Tasks;

namespace BluetoothBase.Bluetooth
{
    public interface IBluetoothAdapter
    {
        void Discover(Func<BluetoothDeviceInfo, Task> discoveryHandler, CancellationToken cancellationToken = default);

        Task<IBluetoothDevice> GetDeviceAsync(ulong bluetoothAddress);
    }
}