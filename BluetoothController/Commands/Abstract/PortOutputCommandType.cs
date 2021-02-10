using BluetoothController.Models;

namespace BluetoothController.Commands.Abstract
{
    public abstract class PortOutputCommandType : CommandType
    {
        public PortOutputCommandType() : base(MessageTypes.PortOutput)
        {
        }
    }
}


