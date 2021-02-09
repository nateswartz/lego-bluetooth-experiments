using BluetoothController.Models;

namespace BluetoothController.Commands.Basic
{
    public abstract class PortOutputCommand : TypedCommand
    {
        public PortOutputCommand() : base(CommandTypes.PortOutput)
        {
        }
    }
}


