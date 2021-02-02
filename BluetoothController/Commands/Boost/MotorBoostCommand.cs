using LegoBoostController.Models;

namespace LegoBoostController.Commands.Boost
{
    public class MotorBoostCommand : IBoostCommand
    {
        public string HexCommand { get; set; }

        public MotorBoostCommand(Motor motor, int powerPercentage = 100, int timeInMS = 1000, bool clockwise = true, string currentExternalMotorPort = "")
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
            HexCommand = $"0c0081{motorToRun}1109{time}{power}647f03";
        }
    }
}


