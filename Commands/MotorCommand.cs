using SDKTemplate.Models;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SDKTemplate.Commands
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
                await controller.RunMotor(motor, speed, time, clockWise);
                await Task.Delay(time);
            }
        }
    }
}


