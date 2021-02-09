using BluetoothController.Controllers;
using BluetoothController.Util;

namespace BluetoothController.Commands.Basic
{
    public class TrainMotorCommand : IPoweredUpCommand
    {
        public string HexCommand { get; set; }

        public TrainMotorCommand(HubController controller, int powerPercentage, bool clockwise)
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

            HexCommand = CommandHelper.AddHeader($"{commandType}{motorToRun}{startupCompletion}{subCommand}00{power}");
        }
    }
}


