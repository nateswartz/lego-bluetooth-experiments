using System;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Foundation;
using Windows.Storage.Streams;

namespace BluetoothController.Wrappers
{
    public class GattCharacteristicWrapper : IGattCharacteristicWrapper
    {
        private GattCharacteristic _gattCharacteristic;

        public GattCharacteristicWrapper(GattCharacteristic gattCharacteristic)
        {
            _gattCharacteristic = gattCharacteristic;
        }

        public async Task<bool> WriteValueWithResultAsync(IBuffer value)
        {
            var result = await _gattCharacteristic.WriteValueWithResultAsync(value);

            return result.Status == GattCommunicationStatus.Success;
        }

        public async Task<bool> WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue clientCharacteristicConfigurationDescriptorValue)
        {
            var status = await _gattCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(clientCharacteristicConfigurationDescriptorValue);
            return status == GattCommunicationStatus.Success;
        }

        public void AddValueChangedHandler(TypedEventHandler<GattCharacteristic, GattValueChangedEventArgs> handler)
        {
            _gattCharacteristic.ValueChanged += handler;
        }

        public void RemoveValueChangedHandler(TypedEventHandler<GattCharacteristic, GattValueChangedEventArgs> handler)
        {
            _gattCharacteristic.ValueChanged -= handler;
        }
    }
}
