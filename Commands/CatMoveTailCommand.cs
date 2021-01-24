using LegoBoostController.Controllers;
using LegoBoostController.Models;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LegoBoostController.Commands
{
    public class CatMoveTailCommand : ICommand
    {
        public IEnumerable<string> Keywords { get => new List<string> { "shaketail" }; }

        public async Task RunAsync(BoostController controller, string commandText)
        {
            Match m = Regex.Match(commandText, @"\((\d+),(\d+),(\w+)\)");
            if (m.Groups.Count == 4)
            {
                var speed = Convert.ToInt32(m.Groups[1].Value);
                var time = Convert.ToInt32(m.Groups[2].Value);
                var direction = m.Groups[3].Value;
                await controller.RunMotor(Motors.A, speed, time, direction == "left");
                await Task.Delay(time);
            }
        }
    }
}


