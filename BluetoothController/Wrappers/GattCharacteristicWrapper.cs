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

        public IAsyncOperation<GattWriteResult> WriteValueWithResultAsync(IBuffer value)
        {
            return _gattCharacteristic.WriteValueWithResultAsync(value);
        }

        public IAsyncOperation<GattCommunicationStatus> WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue clientCharacteristicConfigurationDescriptorValue)
        {
            return _gattCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(clientCharacteristicConfigurationDescriptorValue);
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
