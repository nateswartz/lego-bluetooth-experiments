using SDKTemplate.Commands;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;

namespace SDKTemplate
{
    public class TextCommandsController
    {
        private readonly BoostController _controller;
        private readonly StorageFolder _storageFolder;
        private const string _saveFile = "savedCommands.txt";

        public TextCommandsController(BoostController controller, StorageFolder storageFolder)
        {
            _controller = controller;
            _storageFolder = storageFolder;
        }

        public async Task RunCommandsAsync(string commands)
        {
            if (!String.IsNullOrEmpty(commands))
            {
                var statements = commands.Split(';').Where(c => !string.IsNullOrEmpty(c));
                foreach (var statement in statements)
                {
                    var commandToRun = Regex.Replace(statement.ToLower(), @"\s+", "");
                    var keyword = commandToRun.Split('(')[0];
                    var command = CommandFactory.GetCommand(keyword);
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
