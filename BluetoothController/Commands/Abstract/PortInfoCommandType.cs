using BluetoothController.Models;

namespace BluetoothController.Commands.Abstract
{
    public abstract class PortInfoCommandType : CommandType
    {
        public PortInfoCommandType() : base(MessageTypes.PortInformationRequest)
        {
        }
    }
}


