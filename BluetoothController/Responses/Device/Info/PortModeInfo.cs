using BluetoothController.Models;
using BluetoothController.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BluetoothController.Responses.Device.Info
{
    public enum InputSideMappingFlag
    {
        Discrete = 4,
        Relative = 8,
        Absolute = 16,
        SupportsFunctionalMapping2Plus = 64,
        SupportsNULLValue = 128
    }

    public class PortModeInfo : Response
    {
        public string Port { get; set; }
        public string Mode { get; set; }
        public ModeInfoType ModeInfoType { get; set; }
        public string Value { get; set; }
        public int MinValue { get; set; }
        public int MaxValue { get; set; }
        public List<InputSideMappingFlag> InputSideMappings { get; set; } = new List<InputSideMappingFlag>();

        public PortModeInfo(string body) : base(body)
        {
            Port = body.Substring(6, 2);
            Mode = body.Substring(8, 2);
            ModeInfoType = (ModeInfoType)Convert.ToInt32(body.Substring(10, 2), 16);
            switch (ModeInfoType)
            {
                case (ModeInfoType.Name):
                case (ModeInfoType.Symbol):
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
                    break;
                case (ModeInfoType.Raw):
                case (ModeInfoType.Percent):
                case (ModeInfoType.Si):
                    byte[] minBytes = DataConverter.HexStringToByteArray(body.Substring(12, 8));
                    MinValue = (int)BitConverter.ToSingle(minBytes, 0);

                    byte[] maxBytes = DataConverter.HexStringToByteArray(body.Substring(20, 8));
                    MaxValue = (int)BitConverter.ToSingle(maxBytes, 0);
                    break;
                case (ModeInfoType.Mapping):
                    var inputSideMappingBitfield = (InputSideMappingFlag)Convert.ToInt32(body.Substring(12, 2), 16);
                    foreach (var value in Enum.GetValues(typeof(InputSideMappingFlag)))
                    {
                        var mapping = (InputSideMappingFlag)Enum.Parse(typeof(InputSideMappingFlag), value.ToString());
                        if ((inputSideMappingBitfield & mapping) == mapping)
                            InputSideMappings.Add(mapping);
                    }
                    break;
            }
        }

        public override string ToString()
        {
            var header = $"Port Mode Info ({Port}) " +
                    $"{Environment.NewLine}\tMode: {Mode}; Mode Info Type: {Enum.GetName(typeof(ModeInfoType), ModeInfoType)}";
            var modeSpecific = "";
            if (ModeInfoType == ModeInfoType.Name || ModeInfoType == ModeInfoType.Symbol)
                modeSpecific = $"{Environment.NewLine}\tValue: {Value}";
            else if (ModeInfoType == ModeInfoType.Raw || ModeInfoType == ModeInfoType.Percent || ModeInfoType == ModeInfoType.Si)
                modeSpecific = $"{Environment.NewLine}\tMinValue: {MinValue}; MaxValue: {MaxValue}";
            else if (ModeInfoType == ModeInfoType.Mapping)
                modeSpecific = $"{Environment.NewLine}\tInputSideMappings: {string.Join(", ", InputSideMappings.Select(c => Enum.GetName(typeof(InputSideMappingFlag), c)))}";
            var footer = $"{Environment.NewLine}\t[{Body}]";
            return header + modeSpecific + footer;
        }
    }
}