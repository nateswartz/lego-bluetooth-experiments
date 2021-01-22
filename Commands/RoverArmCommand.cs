using SDKTemplate.Models;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SDKTemplate.Commands
{
    public class RoverArmCommand : ICommand
    {
        public IEnumerable<string> Keywords { get => new List<string> { "raise", "lower" }; }

        public async Task RunAsync(BoostController controller, string commandText)
        {
            Match m = Regex.Match(commandText, @"\((\d+),(\d+)\)");
            if (m.Groups.Count == 3)
            {
                var speed = Convert.ToInt32(m.Groups[1].Value);
                var time = Convert.ToInt32(m.Groups[2].Value);
                var raise = commandText.StartsWith("raise");
                await controller.RunMotor(Motors.External, speed, time, raise);
                await Task.Delay(time);
            }
        }
    }
}


