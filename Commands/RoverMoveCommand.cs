using SDKTemplate.Models;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SDKTemplate.Commands
{
    public class RoverMoveCommand : ICommand
    {
        public IEnumerable<string> Keywords { get => new List<string> { "forward", "back" }; }

        public async Task RunAsync(BoostController controller, string commandText)
        {
            Match m = Regex.Match(commandText, @"\((\d+),(\d+)\)");
            if (m.Groups.Count == 3)
            {
                var speed = Convert.ToInt32(m.Groups[1].Value);
                var time = Convert.ToInt32(m.Groups[2].Value);
                var forward = commandText.StartsWith("forward");
                await controller.RunMotor(Motors.A_B, speed, time, forward);
                await Task.Delay(time);
            }
        }
    }
}


