using BluetoothController.Models;

namespace BluetoothController.Commands.Basic
{
    public abstract class HubActionCommand : TypedCommand
    {
        public HubActionCommand() : base(CommandTypes.HubAction)
        {
        }
    }
}


