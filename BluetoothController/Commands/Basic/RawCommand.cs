namespace BluetoothController.Commands.Basic
{
    public class RawCommand : ICommand
    {
        public string HexCommand { get; set; }

        public RawCommand(string command)
        {
            HexCommand = command;
        }
    }
}


