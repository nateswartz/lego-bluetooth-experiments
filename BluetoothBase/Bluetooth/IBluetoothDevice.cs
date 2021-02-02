using System;
using System.Threading.Tasks;

namespace BluetoothBase.Bluetooth
{
    public interface IBluetoothDevice : IDisposable
    {
        string Name { get; }
        Task<IBluetoothService> GetServiceAsync(Guid serviceId);
    }
}