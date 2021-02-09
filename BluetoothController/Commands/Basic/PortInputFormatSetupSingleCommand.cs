using BluetoothController.Models;

namespace BluetoothController.Commands.Basic
{
    public abstract class PortInputFormatSetupSingleCommand : TypedCommand
    {
        public PortInputFormatSetupSingleCommand() : base(CommandTypes.PortInputFormatSetupSingle)
        {
        }
    }
}


