using BluetoothController.Models;
using BluetoothController.Util;

namespace BluetoothController.Commands.Basic
{
    public class MotorCommand : IPoweredUpCommand
    {
        public string HexCommand { get; set; }

        public MotorCommand(Motor motor, int powerPercentage = 100, int timeInMS = 1000, bool clockwise = true, string currentExternalMotorPort = "")
        {
            string motorToRun = motor.Code;
            if (motor == Motors.External)
            {
                motorToRun = currentExternalMotorPort;
            }

            // For time, LSB first
            var time = timeInMS.ToString("X").PadLeft(4, '0');
            time = $"{time[2]}{time[3]}{time[0]}{time[1]}";
            var power = "";
            if (clockwise)
            {
                power = powerPercentage.ToString("X");
            }
            else
            {
                power = (255 - powerPercentage).ToString("X");
            }
            power = power.PadLeft(2, '0');
            HexCommand = CommandHelper.AddHeader($"81{motorToRun}1109{time}{power}647f03)");
        }
    }
}


