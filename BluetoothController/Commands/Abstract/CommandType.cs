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
            var numBytes = (command.Length + 6) / 2;
            return $"{numBytes:X2}00{MessageType}{command}";
        }
    }
}


