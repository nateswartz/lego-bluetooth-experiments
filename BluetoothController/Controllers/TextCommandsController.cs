using BluetoothController.Commands.Robot;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BluetoothController.Controllers
{
    public enum Robot
    {
        Rover = 1,
        Cat = 2
    }

    public class TextCommandsController
    {
        private readonly HubController _controller;
        private ICommandFactory _commandFactory;
        private Robot selectedRobot;
        public Robot SelectedRobot
        {
            get { return selectedRobot; }
            set
            {
                selectedRobot = value;
                if (selectedRobot == Robot.Rover)
                    SampleCommandsText = new RoverCommandFactory().GetCommandExamples();
                if (selectedRobot == Robot.Cat)
                    SampleCommandsText = new CatCommandFactory().GetCommandExamples();
            }
        }
        public string SampleCommandsText { get; set; }

        public TextCommandsController(HubController controller)
        {
            _controller = controller;
        }

        public async Task RunCommandsAsync(string commands)
        {
            if (SelectedRobot == Robot.Rover)
            {
                _commandFactory = new RoverCommandFactory();
            }
            else if (SelectedRobot == Robot.Cat)
            {
                _commandFactory = new CatCommandFactory();
            }

            if (!string.IsNullOrEmpty(commands))
            {
                var statements = commands.Split(';').Where(c => !string.IsNullOrEmpty(c));
                foreach (var statement in statements)
                {
                    var commandToRun = Regex.Replace(statement.ToLower(), @"\s+", "");
                    var keyword = commandToRun.Split('(')[0];
                    var command = _commandFactory.GetCommand(keyword);
                    await command.RunAsync(_controller, commandToRun);
                    await Task.Delay(500);
                }
            }
        }
    }
}
