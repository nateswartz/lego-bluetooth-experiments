using BluetoothController.Commands.Abstract;
using BluetoothController.Models;
using BluetoothController.Models.Enums;

namespace BluetoothController.Commands.Basic
{

    public class PortInfoCommand : CommandType
    {
        public PortInfoCommand(string portId, InfoType infoType) : base(MessageTypes.PortInformationRequest)
        {
            HexCommand = AddHeader($"{portId}{(int)infoType:X2}");
        }
    }
}


