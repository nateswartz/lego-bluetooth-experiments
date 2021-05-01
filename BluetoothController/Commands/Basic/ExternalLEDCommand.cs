using BluetoothController.Commands.Abstract;

namespace BluetoothController.Commands.Basic
{
    public class ExternalLEDCommand : PortOutputCommandType
    {
        public ExternalLEDCommand(string portId, int powerPercentage)
        {
            HexCommand = AddHeader($"{portId}115100{powerPercentage:X2}");
        }
    }
}
