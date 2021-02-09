namespace BluetoothController.Commands.Boost
{
    public class DisconnectCommand : HubActionCommand, IBoostCommand
    {
        public string HexCommand { get; set; }

        public DisconnectCommand()
        {
            var actionType = "02"; // Disconnect
            HexCommand = AddHeader($"{actionType}"); ;
        }
    }
}


