namespace BluetoothController.Commands.Basic
{
    public class ShutdownCommand : HubActionCommand, IPoweredUpCommand
    {
        public string HexCommand { get; set; }

        public ShutdownCommand()
        {
            var actionType = "01"; // Shutdown
            HexCommand = AddHeader($"{actionType}");
        }
    }
}


