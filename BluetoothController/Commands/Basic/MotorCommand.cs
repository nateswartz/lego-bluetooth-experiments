using BluetoothController.Commands.Abstract;

namespace BluetoothController.Commands.Basic
{
    public class MotorCommand : PortOutputCommandType, ICommand
    {
        public string HexCommand { get; set; }

        public MotorCommand(string port, int powerPercentage = 100, int timeInMS = 1000, bool clockwise = true)
        {
            // For time, LSB first
            var time = $"{timeInMS:X4}";
            time = $"{time[2]}{time[3]}{time[0]}{time[1]}";
            string power;
            if (clockwise)
            {
                power = $"{powerPercentage:X2}";
            }
            else
            {
                power = $"{(255 - powerPercentage):X2}";
            }
            HexCommand = AddHeader($"{port}1109{time}{power}647f03");
        }
    }
}


