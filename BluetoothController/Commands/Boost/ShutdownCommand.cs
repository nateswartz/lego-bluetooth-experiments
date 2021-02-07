using BluetoothController.Util;

namespace BluetoothController.Commands.Boost
{
    public class ShutdownCommand : IBoostCommand
    {
        public string HexCommand { get; set; }

        public ShutdownCommand()
        {
            var messageType = "02"; // Hub Action
            var actionType = "01"; // Disconnect
            HexCommand = CommandHelper.AddHeader($"{messageType}{actionType}");
        }
    }
}


