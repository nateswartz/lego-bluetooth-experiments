using BluetoothController.Commands.Abstract;
using BluetoothController.Models;

namespace BluetoothController.Commands.Basic
{
    public enum ModeInfoType
    {
        Name = 0,
        Raw = 1,
        Percent = 2,
        Si = 3,
        Symbol = 4,
        Mapping = 5,
        Internal = 6,
        MotorBias = 7,
        CapabilityBits = 8,
        ValueFormat = 128
    }

    public class PortInfoModeCommand : CommandType
    {
        public PortInfoModeCommand(string portId, string mode, ModeInfoType type) : base(MessageTypes.PortModeInformationRequest)
        {
            HexCommand = AddHeader($"{portId}{mode}{(int)type:X2}");
        }
    }
}


