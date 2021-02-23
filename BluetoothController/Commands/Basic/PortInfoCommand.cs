using BluetoothController.Commands.Abstract;

namespace BluetoothController.Commands.Basic
{
    public enum InfoType
    {
        PortValue = 0,
        ModeInfo = 1,
        PossibleModeCombinations = 2
    }

    public class PortInfoCommand : PortInfoCommandType
    {
        public PortInfoCommand(string portId, InfoType infoType)
        {
            HexCommand = AddHeader($"{portId}{(int)infoType:X2}");
        }
    }
}


