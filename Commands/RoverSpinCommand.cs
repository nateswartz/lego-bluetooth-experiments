using SDKTemplate.Models;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SDKTemplate.Commands
{
    public class RoverSpinCommand : ICommand
    {
        public IEnumerable<string> Keywords { get => new List<string> { "spin" }; }

        public async Task RunAsync(BoostController controller, string commandText)
        {
            Match m = Regex.Match(commandText, @"\((\d+),(\d+),(\w+)\)");
            if (m.Groups.Count == 4)
            {
                var speed = Convert.ToInt32(m.Groups[1].Value);
                var time = Convert.ToInt32(m.Groups[2].Value);
                var direction = m.Groups[3].Value;
                var motor = direction == "clockwise" ? Motors.A : Motors.B;
                await controller.RunMotor(motor, speed, time, true);
                await Task.Delay(time);
            }
        }
    }
}


