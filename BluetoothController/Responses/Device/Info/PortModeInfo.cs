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
        public int MinValue { get; set; }
        public int MaxValue { get; set; }

        public PortModeInfo(string body) : base(body)
        {
            Port = body.Substring(6, 2);
            Mode = body.Substring(8, 2);
            ModeInfoType = (ModeInfoType)Convert.ToInt32(body.Substring(10, 2), 16);
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
                var data = DataConverter.HexStringToByteArray(body.Substring(12, byteCount * 2));
                Value = Encoding.ASCII.GetString(data);
            }
            if (ModeInfoType == ModeInfoType.Raw || ModeInfoType == ModeInfoType.Percent)
            {
                byte[] minBytes = DataConverter.HexStringToByteArray(body.Substring(12, 8));
                MinValue = (int)BitConverter.ToSingle(minBytes, 0);

                byte[] maxBytes = DataConverter.HexStringToByteArray(body.Substring(20, 8));
                MaxValue = (int)BitConverter.ToSingle(maxBytes, 0);
            }
        }

        public override string ToString()
        {
            var header = $"Port Mode Info ({Port}) " +
                    $"{Environment.NewLine}\tMode: {Mode}; Mode Info Type: {Enum.GetName(typeof(ModeInfoType), ModeInfoType)}";
            var modeSpecific = "";
            if (ModeInfoType == ModeInfoType.Name)
                modeSpecific = $"{Environment.NewLine}\tValue: {Value}";
            else if (ModeInfoType == ModeInfoType.Raw || ModeInfoType == ModeInfoType.Percent)
                modeSpecific = $"{Environment.NewLine}\tMinValue: {MinValue}; MaxValue: {MaxValue}";
            var footer = $"{Environment.NewLine}\t[{Body}]";
            return header + modeSpecific + footer;
        }
    }
}