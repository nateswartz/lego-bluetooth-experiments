using BluetoothController.Commands.Abstract;

namespace BluetoothController.Commands.Basic
{
    public class ExternalLedCommand : PortOutputCommandType
    {
        public ExternalLedCommand(string portId, int powerPercentage)
        {
            HexCommand = AddHeader($"{portId}115100{powerPercentage:X2}");
        }
    }
}
