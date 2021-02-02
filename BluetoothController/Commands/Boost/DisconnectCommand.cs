namespace BluetoothController.Commands.Boost
{
    public class DisconnectCommand : IBoostCommand
    {
        public string HexCommand { get; set; }

        public DisconnectCommand()
        {
            var messageLength = "04";
            var messageType = "02"; // Hub Action
            var actionType = "02"; // Disconnect
            HexCommand = $"{messageLength}00{messageType}{actionType}";
        }
    }
}


