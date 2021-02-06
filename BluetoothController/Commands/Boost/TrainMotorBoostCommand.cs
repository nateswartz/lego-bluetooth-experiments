using BluetoothController.Controllers;

namespace BluetoothController.Commands.Boost
{
    public class TrainMotorBoostCommand : IBoostCommand
    {
        public string HexCommand { get; set; }

        public TrainMotorBoostCommand(HubController controller, int powerPercentage, bool clockwise)
        {
            string commandType = "81"; // Port Output Command
            string motorToRun = controller.PortState.CurrentTrainMotorPort;
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

            HexCommand = $"0800{commandType}{motorToRun}{startupCompletion}{subCommand}00{power}";
        }
    }
}


