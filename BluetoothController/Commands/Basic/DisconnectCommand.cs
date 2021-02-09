namespace BluetoothController.Commands.Basic
{
    public class DisconnectCommand : HubActionCommand, IPoweredUpCommand
    {
        public string HexCommand { get; set; }

        public DisconnectCommand()
        {
            var actionType = "02"; // Disconnect
            HexCommand = AddHeader($"{actionType}"); ;
        }
    }
}


