using LegoBoostController.Controllers;
using LegoBoostController.Models;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LegoBoostController.Commands
{
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


