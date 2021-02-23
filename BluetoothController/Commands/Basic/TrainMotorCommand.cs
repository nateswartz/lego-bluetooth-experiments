using BluetoothController.Commands.Abstract;

namespace BluetoothController.Commands.Basic
{
    public class TrainMotorCommand : PortOutputCommandType
    {
        public TrainMotorCommand(string port, int powerPercentage, bool clockwise)
        {
            string startupCompletion = "11"; // Execute immediately / Command feedback
            string subCommand = "51"; // WriteDirectModeData
            string power;
            if (clockwise && powerPercentage != 0)
            {
                power = $"{powerPercentage:X2}";
            }
            else
            {
                power = $"{(255 - powerPercentage):X2}";
            }

            HexCommand = AddHeader($"{port}{startupCompletion}{subCommand}00{power}");
        }
    }
}


