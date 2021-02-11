using BluetoothController.Commands.Abstract;

namespace BluetoothController.Commands.Basic
{
    public class MotorCommand : PortOutputCommandType, IPoweredUpCommand
    {
        public string HexCommand { get; set; }

        public MotorCommand(string port, int powerPercentage = 100, int timeInMS = 1000, bool clockwise = true)
        {
            // For time, LSB first
            var time = timeInMS.ToString("X").PadLeft(4, '0');
            time = $"{time[2]}{time[3]}{time[0]}{time[1]}";
            string power;
            if (clockwise)
            {
                power = powerPercentage.ToString("X");
            }
            else
            {
                power = (255 - powerPercentage).ToString("X");
            }
            power = power.PadLeft(2, '0');
            HexCommand = AddHeader($"{port}1109{time}{power}647f03");
        }
    }
}


