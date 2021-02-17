using BluetoothController.Commands.Abstract;

namespace BluetoothController.Commands.Basic
{
    public class TrainMotorCommand : PortOutputCommandType, IPoweredUpCommand
    {
        public string HexCommand { get; set; }

        public TrainMotorCommand(string port, int powerPercentage, bool clockwise)
        {
            string startupCompletion = "11"; // Execute immediately / Command feedback
            string subCommand = "51"; // WriteDirectModeData
            string power;
            if (clockwise && powerPercentage != 0)
            {
                power = powerPercentage.ToString("X");
            }
            else
            {
                power = (255 - powerPercentage).ToString("X");
            }

            HexCommand = AddHeader($"{port}{startupCompletion}{subCommand}00{power}");
        }
    }
}


