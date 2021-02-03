using BluetoothController.Commands.Boost;
using BluetoothController.Controllers;
using BluetoothController.Models;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BluetoothController.Commands.Robot
{
    public abstract class MotorCommand
    {
        public async Task RunAsync(HubController controller, string commandText, string clockwiseKeyword, Motor motor)
        {
            Match m = Regex.Match(commandText, @"\((\d+),(\d+)\)");
            if (m.Groups.Count == 3)
            {
                var speed = Convert.ToInt32(m.Groups[1].Value);
                var time = Convert.ToInt32(m.Groups[2].Value);
                var clockWise = commandText.StartsWith(clockwiseKeyword);
                var command = new MotorBoostCommand(Motors.External, speed, time, clockWise, controller.GetCurrentExternalMotorPort());
                await controller.ExecuteCommandAsync(command);
                await Task.Delay(time);
            }
        }
    }
}


