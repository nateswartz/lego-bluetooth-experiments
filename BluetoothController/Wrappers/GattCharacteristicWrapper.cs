using System;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace BluetoothController.Wrappers
{
    public class GattCharacteristicWrapper : IGattCharacteristicWrapper
    {
        private GattCharacteristic _gattCharacteristic;
        private Func<IBuffer, Task> _charactersticChangedCallback;

        public GattCharacteristicWrapper(GattCharacteristic gattCharacteristic)
        {
            _gattCharacteristic = gattCharacteristic;

        }

        public async Task<bool> WriteValueWithResultAsync(IBuffer value)
        {
            var result = await _gattCharacteristic.WriteValueWithResultAsync(value);

            return result.Status == GattCommunicationStatus.Success;
        }

        public async Task<bool> EnableNotificationsAsync()
        {
            var status = await _gattCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
            return status == GattCommunicationStatus.Success;
        }

        public void AddValueChangedHandler(Func<IBuffer, Task> charactersticChangedCallback)
        {
            _charactersticChangedCallback = charactersticChangedCallback;
            _gattCharacteristic.ValueChanged += Characteristic_ValueChanged;
        }

        public void RemoveValueChangedHandler()
        {
            _gattCharacteristic.ValueChanged -= Characteristic_ValueChanged;
        }

        private async void Characteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            await _charactersticChangedCallback(args.CharacteristicValue);
        }
    }
}
