using BluetoothController.Commands.Abstract;
using BluetoothController.Util;

namespace BluetoothController.Commands.Basic
{
    public class TrainMotorCommand : PortOutputCommandType
    {
        public TrainMotorCommand(string port, int powerPercentage, bool clockwise)
        {
            string startupCompletion = "11"; // Execute immediately / Command feedback
            string subCommand = "51"; // WriteDirectModeData
            var power = DataConverter.PowerPercentageToHex(powerPercentage, clockwise);
            HexCommand = AddHeader($"{port}{startupCompletion}{subCommand}00{power}");
        }
    }
}


