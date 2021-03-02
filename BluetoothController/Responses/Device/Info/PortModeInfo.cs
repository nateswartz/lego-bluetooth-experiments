using BluetoothController.Models;
using BluetoothController.Util;
using System;
using System.Text;

namespace BluetoothController.Responses.Device.Info
{

    public class PortModeInfo : Response
    {
        public string Port { get; set; }
        public string Mode { get; set; }
        public ModeInfoType ModeInfoType { get; set; }
        public string Value { get; set; }

        public PortModeInfo(string body) : base(body)
        {
            Port = body.Substring(6, 2);
            Mode = body.Substring(8, 2);
            ModeInfoType = (ModeInfoType)Convert.ToInt32(body.Substring(10, 2), 16);
            // TODO: Verify this behavior
            if (ModeInfoType == ModeInfoType.Name)
            {
                var index = 12;
                var byteCount = 0;
                var nextByte = body.Substring(index, 2);
                while (nextByte != "00")
                {
                    index += 2;
                    byteCount++;
                    nextByte = body.Substring(index, 2);
                }
                var data = DataConverter.HexStringToByteArray(Body.Substring(12, byteCount * 2));
                Value = Encoding.ASCII.GetString(data);
            }
        }

        public override string ToString()
        {
            return $"Port Mode Info ({Port})" +
                    $"{Environment.NewLine}\tMode: {Mode}; Mode Info Type: {Enum.GetName(typeof(ModeInfoType), ModeInfoType)}" +
                    $"{Environment.NewLine}\tValue: {Value}" +
                    $"{Environment.NewLine}\t[{Body}]";
        }
    }
}