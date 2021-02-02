using System;
using System.Threading.Tasks;

namespace BluetoothBase.Bluetooth
{
    public interface IBluetoothService : IDisposable
    {
        Guid Uuid { get; }
        Task<IBluetoothCharacteristic> GetCharacteristicAsync(Guid guid);
    }
}