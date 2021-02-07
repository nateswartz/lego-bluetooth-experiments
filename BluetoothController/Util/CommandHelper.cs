namespace BluetoothController.Util
{
    public static class CommandHelper
    {
        public static string AddHeader(string command)
        {
            var numBytes = (command.Length + 4) / 2;
            return $"{numBytes:X2}00{command}";
        }
    }
}
