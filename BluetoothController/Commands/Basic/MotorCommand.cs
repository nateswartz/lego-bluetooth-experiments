using BluetoothController.Commands.Abstract;
using BluetoothController.Util;

namespace BluetoothController.Commands.Basic
{
    public class MotorCommand : PortOutputCommandType
    {
        public MotorCommand(string port, int powerPercentage = 100, int timeInMS = 1000, bool clockwise = true)
        {
            var time = DataConverter.MillisecondsToHex(timeInMS);
            var power = DataConverter.PowerPercentageToHex(powerPercentage, clockwise);
            HexCommand = AddHeader($"{port}1109{time}{power}647f03");
        }
    }
}


