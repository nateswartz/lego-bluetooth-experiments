using BluetoothController.Models;

namespace BluetoothController.Commands.Abstract
{
    public abstract class DeviceInfoCommandType : CommandType
    {
        public DeviceInfoCommandType() : base(MessageTypes.HubProperty)
        {
        }
    }
}


