using BluetoothController.Commands.Basic;
using BluetoothController.Controllers;
using LegoBoostController.Models;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LegoBoostController.Robot
{
    public abstract class MotorRobotCommand
    {
        public async Task RunAsync(HubController controller, string commandText, string clockwiseKeyword, Motor motor)
        {
            Match m = Regex.Match(commandText, @"\((\d+),(\d+)\)");
            if (m.Groups.Count == 3)
            {
                var speed = Convert.ToInt32(m.Groups[1].Value);
                var time = Convert.ToInt32(m.Groups[2].Value);
                var clockWise = commandText.StartsWith(clockwiseKeyword);
                var port = motor.Name == "External" ? controller.GetCurrentExternalMotorPort() : motor.Code;
                var command = new MotorCommand(port, speed, time, clockWise);
                await controller.ExecuteCommandAsync(command);
                await Task.Delay(time);
            }
        }
    }
}


