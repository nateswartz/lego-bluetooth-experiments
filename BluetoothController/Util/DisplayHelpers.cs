using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace BluetoothController.Util
{
    /// <summary>
    ///     Represents the display of an attribute - both characteristics and services.
    /// </summary>
    public class BluetoothLEAttributeDisplay
    {
        public GattCharacteristic characteristic;
        public GattDescriptor descriptor;

        public GattDeviceService service;

        public BluetoothLEAttributeDisplay(GattDeviceService service)
        {
            this.service = service;
            AttributeDisplayType = AttributeType.Service;
        }

        public BluetoothLEAttributeDisplay(GattCharacteristic characteristic)
        {
            this.characteristic = characteristic;
            AttributeDisplayType = AttributeType.Characteristic;
        }

        public string Name
        {
            get
            {
                switch (AttributeDisplayType)
                {
                    case AttributeType.Service:
                        return "Custom Service: " + service.Uuid;
                    case AttributeType.Characteristic:
                        if (!string.IsNullOrEmpty(characteristic.UserDescription))
                        {
                            return characteristic.UserDescription;
                        }

                        else
                        {
                            return "Custom Characteristic: " + characteristic.Uuid;
                        }
                    default:
                        break;
                }
                return "Invalid";
            }
        }

        public AttributeType AttributeDisplayType { get; }

    }

    public enum AttributeType
    {
        Service = 0,
        Characteristic = 1,
        Descriptor = 2
    }
}