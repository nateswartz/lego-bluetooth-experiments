using BluetoothController.Commands.Abstract;

namespace BluetoothController.Commands.Basic
{
    public class DisconnectCommand : HubActionCommandType, ICommand
    {
        public string HexCommand { get; set; }

        public DisconnectCommand()
        {
            var actionType = "02"; // Disconnect
            HexCommand = AddHeader($"{actionType}"); ;
        }
    }
}


