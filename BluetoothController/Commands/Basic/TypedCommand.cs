using BluetoothController.Util;

namespace BluetoothController.Commands.Basic
{
    public abstract class TypedCommand
    {
        public string MessageType { get; set; }

        public TypedCommand(string messageType)
        {
            MessageType = messageType;
        }
        public string AddHeader(string command)
        {
            return CommandHelper.AddHeader($"{MessageType}{command}");
        }
    }
}


