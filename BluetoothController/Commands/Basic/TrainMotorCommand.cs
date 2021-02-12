using BluetoothController.Commands.Abstract;
using BluetoothController.Hubs;

namespace BluetoothController.Commands.Basic
{
    public class TrainMotorCommand : PortOutputCommandType, IPoweredUpCommand
    {
        public string HexCommand { get; set; }

        public TrainMotorCommand(ModularHub hub, int powerPercentage, bool clockwise)
        {
            string motorToRun = hub.CurrentTrainMotorPort;
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

            HexCommand = AddHeader($"{motorToRun}{startupCompletion}{subCommand}00{power}");
        }
    }
}


