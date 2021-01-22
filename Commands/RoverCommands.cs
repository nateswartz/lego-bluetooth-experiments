using SDKTemplate.Models;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SDKTemplate.Commands
{
    public interface ICommand
    {
        IEnumerable<string> Keywords { get; }
        Task RunAsync(BoostController controller, string commandText);
    }

    public class MoveCommand : ICommand
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

    public class SpinCommand : ICommand
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

    public class ArmCommand : ICommand
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

    public class LEDCommand : ICommand
    {
        public IEnumerable<string> Keywords { get => new List<string> { "led" }; }

        public async Task RunAsync(BoostController controller, string commandText)
        {
            Match m = Regex.Match(commandText, @"\((\w+)\)");
            if (m.Groups.Count == 2)
            {
                var color = m.Groups[1].Value;
                await controller.SetLEDColor(LEDColors.GetByName(color));
            }
        }
    }
}


