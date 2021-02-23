using BluetoothController.Commands.Basic;
using BluetoothController.Models;

namespace BluetoothController.Commands.Abstract
{
    public abstract class CommandType : ICommand
    {
        public string HexCommand { get; set; }

        public MessageType MessageType { get; set; }

        public CommandType(MessageType messageType)
        {
            MessageType = messageType;
        }

        public string AddHeader(string command)
        {
            var numBytes = (command.Length + 6) / 2;
            return $"{numBytes:X2}00{MessageType}{command}";
        }
    }
}


