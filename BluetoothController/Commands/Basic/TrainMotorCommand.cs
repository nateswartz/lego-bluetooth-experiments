using BluetoothController.Controllers;

namespace BluetoothController.Commands.Basic
{
    public class TrainMotorCommand : PortOutputCommand, IPoweredUpCommand
    {
        public string HexCommand { get; set; }

        public TrainMotorCommand(HubController controller, int powerPercentage, bool clockwise)
        {
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

            HexCommand = AddHeader($"{motorToRun}{startupCompletion}{subCommand}00{power}");
        }
    }
}


