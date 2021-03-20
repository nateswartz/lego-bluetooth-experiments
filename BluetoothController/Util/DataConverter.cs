using System;
using System.Linq;
using System.Text;

namespace BluetoothController.Util
{
    internal static class DataConverter
    {
        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        public static byte[] HexStringToByteArray(string hex)
        {
            hex = hex.Replace(" ", string.Empty);
            try
            {
                return Enumerable.Range(0, hex.Length)
                  .Where(x => x % 2 == 0)
                  .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                  .ToArray();
            }
            catch (Exception)
            {
                throw new ArgumentException($"Invalid hex command provided: {hex}");
            }
        }

        public static string MillisecondsToHex(int milliseconds)
        {
            var time = $"{milliseconds:X4}";
            // For time, LSB first
            return $"{time[2]}{time[3]}{time[0]}{time[1]}";
        }

        public static string PowerPercentageToHex(int powerPercentage, bool runMotorClockwise)
        {
            if (runMotorClockwise && powerPercentage != 0)
            {
                return $"{powerPercentage:X2}";
            }
            else
            {
                return $"{(255 - powerPercentage):X2}";
            }
        }
    }
}
