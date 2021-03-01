using BluetoothController.Commands.Abstract;
using BluetoothController.Models;

namespace BluetoothController.Commands.Basic
{
    public enum InfoType
    {
        PortValue = 0,
        ModeInfo = 1,
        PossibleModeCombinations = 2
    }

    public class PortInfoCommand : CommandType
    {
        public PortInfoCommand(string portId, InfoType infoType) : base(MessageTypes.PortInformationRequest)
        {
            HexCommand = AddHeader($"{portId}{(int)infoType:X2}");
        }
    }
}


