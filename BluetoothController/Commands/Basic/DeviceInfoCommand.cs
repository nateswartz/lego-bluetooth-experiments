using BluetoothController.Models;

namespace BluetoothController.Commands.Basic
{
    public abstract class DeviceInfoCommand : TypedCommand
    {
        public DeviceInfoCommand() : base(CommandTypes.DeviceInfo)
        {
        }
    }
}


