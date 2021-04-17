using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Foundation;
using Windows.Storage.Streams;

namespace BluetoothController.Wrappers
{
    public interface IGattCharacteristicWrapper
    {
        void AddValueChangedHandler(TypedEventHandler<GattCharacteristic, GattValueChangedEventArgs> handler);
        void RemoveValueChangedHandler(TypedEventHandler<GattCharacteristic, GattValueChangedEventArgs> handler);
        IAsyncOperation<GattCommunicationStatus> WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue clientCharacteristicConfigurationDescriptorValue);
        IAsyncOperation<GattWriteResult> WriteValueWithResultAsync(IBuffer value);
    }
}