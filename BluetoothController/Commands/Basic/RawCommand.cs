namespace BluetoothController.Commands.Basic
{
    public class RawCommand : IPoweredUpCommand
    {
        public string HexCommand { get; set; }

        public RawCommand(string command)
        {
            HexCommand = command;
        }
    }
}


