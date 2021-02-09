namespace BluetoothController.Commands.Boost
{
    public class ShutdownCommand : HubActionCommand, IBoostCommand
    {
        public string HexCommand { get; set; }

        public ShutdownCommand()
        {
            var actionType = "01"; // Shutdown
            HexCommand = AddHeader($"{actionType}");
        }
    }
}


