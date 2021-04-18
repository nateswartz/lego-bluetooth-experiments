using System;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace BluetoothController.Wrappers
{
    public interface IGattCharacteristicWrapper
    {
        void AddValueChangedHandler(Func<IBuffer, Task> charactersticChangedCallback);
        void RemoveValueChangedHandler();
        Task<bool> EnableNotificationsAsync();
        Task<bool> WriteValueWithResultAsync(IBuffer value);
    }
}