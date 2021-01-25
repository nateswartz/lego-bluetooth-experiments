using LegoBoostController.Commands.Boost;
using LegoBoostController.Controllers;
using LegoBoostController.Models;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LegoBoostController.Commands.Robot
{
    public abstract class MotorCommand
    {
        public async Task RunAsync(BoostController controller, string commandText, string clockwiseKeyword, Motor motor)
        {
            Match m = Regex.Match(commandText, @"\((\d+),(\d+)\)");
            if (m.Groups.Count == 3)
            {
                var speed = Convert.ToInt32(m.Groups[1].Value);
                var time = Convert.ToInt32(m.Groups[2].Value);
                var clockWise = commandText.StartsWith(clockwiseKeyword);
                var command = new MotorBoostCommand(Motors.External, speed, time, clockWise, controller.GetCurrentExternalMotorPort());
                await controller.SetHexValueAsync(command);
                await Task.Delay(time);
            }
        }
    }
}


