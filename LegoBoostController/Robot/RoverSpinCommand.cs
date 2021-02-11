using BluetoothController.Commands.Basic;
using BluetoothController.Controllers;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LegoBoostController.Robot
{
    public class RoverSpinCommand : IRobotCommand
    {
        public IEnumerable<string> Keywords { get => new List<string> { "spin" }; }

        public string Description { get => "Spin(Speed, Time, Direction[Clockwise/CounterClockwise])"; }

        public async Task RunAsync(HubController controller, string commandText)
        {
            Match m = Regex.Match(commandText, @"\((\d+),(\d+),(\w+)\)");
            if (m.Groups.Count == 4)
            {
                var speed = Convert.ToInt32(m.Groups[1].Value);
                var time = Convert.ToInt32(m.Groups[2].Value);
                var direction = m.Groups[3].Value;
                var port = direction == "clockwise" ? "00" : "01";
                var command = new MotorCommand(port, speed, time, true);
                await controller.ExecuteCommandAsync(command);
                await Task.Delay(time);
            }
        }
    }
}


