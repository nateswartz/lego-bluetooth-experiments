using System;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace BluetoothController.Wrappers
{
    public interface IGattCharacteristicWrapper
    {
        void AddValueChangedCallback(Func<IBuffer, Task> charactersticChangedCallback);
        void RemoveValueChangedCallback();
        Task<bool> EnableNotificationsAsync();
        Task<bool> WriteValueAsync(IBuffer value);
    }
}