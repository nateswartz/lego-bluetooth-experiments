using BluetoothController.Util;

namespace BluetoothController.Commands.Boost
{
    public class DisconnectCommand : IBoostCommand
    {
        public string HexCommand { get; set; }

        public DisconnectCommand()
        {
            var messageType = "02"; // Hub Action
            var actionType = "02"; // Disconnect
            HexCommand = CommandHelper.AddHeader($"{messageType}{actionType}"); ;
        }
    }
}


