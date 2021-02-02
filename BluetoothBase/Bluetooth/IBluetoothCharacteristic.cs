using System;
using System.Threading.Tasks;

namespace BluetoothBase.Bluetooth
{
    public interface IBluetoothCharacteristic
    {
        Guid Uuid { get; }
        Task<bool> NotifyValueChangeAsync(Func<byte[], Task> notificationHandler);
        Task<bool> WriteValueAsync(byte[] data);
    }
}