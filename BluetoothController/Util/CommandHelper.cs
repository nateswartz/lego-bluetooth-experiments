namespace BluetoothController.Util
{
    public static class CommandHelper
    {
        public static string AddHeader(string command)
        {
            command += "00";
            var numBytes = command.Length / 2;
            return $"{numBytes:X2}{command}";
        }
    }
}
