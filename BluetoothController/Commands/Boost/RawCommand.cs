namespace BluetoothController.Commands.Boost
{
    public class RawCommand : IBoostCommand
    {
        public string HexCommand { get; set; }

        public RawCommand(string command)
        {
            HexCommand = command;
        }
    }
}


