using BluetoothController.Util;

namespace BluetoothController.Commands.Abstract
{
    public abstract class CommandType
    {
        public string MessageType { get; set; }

        public CommandType(string messageType)
        {
            MessageType = messageType;
        }
        public string AddHeader(string command)
        {
            return CommandHelper.AddHeader($"{MessageType}{command}");
        }
    }
}


