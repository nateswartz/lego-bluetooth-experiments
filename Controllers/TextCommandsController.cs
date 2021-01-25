using LegoBoostController.Commands.Robot;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;

namespace LegoBoostController.Controllers
{
    public enum Robot
    {
        Rover = 1,
        Cat = 2
    }

    public class TextCommandsController
    {
        private readonly BoostController _controller;
        private readonly StorageFolder _storageFolder;
        private ICommandFactory _commandFactory;
        private const string _saveFile = "savedCommands.txt";
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

        public TextCommandsController(BoostController controller, StorageFolder storageFolder)
        {
            _controller = controller;
            _storageFolder = storageFolder;
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

        public async Task SaveCommandsAsync(string commands)
        {
            var saveFile = await _storageFolder.CreateFileAsync(_saveFile, CreationCollisionOption.OpenIfExists);
            await FileIO.WriteTextAsync(saveFile, commands);
        }

        public async Task<string> LoadCommandsAsync()
        {
            try
            {
                var saveFile = await _storageFolder.GetFileAsync(_saveFile);
                return await FileIO.ReadTextAsync(saveFile);
            }
            catch (IOException)
            {
                return "";
            }

        }
    }
}
