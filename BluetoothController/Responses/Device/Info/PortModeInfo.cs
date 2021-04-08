using BluetoothController.Models.Enums;
using BluetoothController.Util;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public IEnumerable<PortModeInfoMappingFlag> InputSideMappings { get; set; }
        public IEnumerable<PortModeInfoMappingFlag> OutputSideMappings { get; set; }

        public PortModeInfo(string body) : base(body)
        {
            Port = body.Substring(6, 2);
            Mode = body.Substring(8, 2);
            ModeInfoType = (ModeInfoType)Convert.ToInt32(body.Substring(10, 2), 16);
            switch (ModeInfoType)
            {
                case (ModeInfoType.Name):
                case (ModeInfoType.Symbol):
                    Value = ExtractTextValue(body, 12, "00");
                    break;
                case (ModeInfoType.Raw):
                case (ModeInfoType.Percent):
                case (ModeInfoType.Si):
                    MinValue = ExtractFloatAsInt(body, 12, 8);
                    MaxValue = ExtractFloatAsInt(body, 20, 8);
                    break;
                case (ModeInfoType.Mapping):
                    InputSideMappings = ExtractMappingFlags(body, 12, 2);
                    OutputSideMappings = ExtractMappingFlags(body, 14, 2);
                    break;
                case (ModeInfoType.MotorBias):
                    Value = Convert.ToInt32(body.Substring(12, 2), 16).ToString();
                    break;
                case (ModeInfoType.CapabilityBits):
                    Value = body.Substring(12, 6);
                    break;
                case (ModeInfoType.ValueFormat):
                    Value = body.Substring(12, 4);
                    break;
            }
        }

        public override string ToString()
        {
            var header = $"Port Mode Info ({Port}) " +
                    $"{Environment.NewLine}\tMode: {Mode}; Mode Info Type: {ModeInfoType}";
            var modeSpecific = "";
            if (ModeInfoType == ModeInfoType.Name || ModeInfoType == ModeInfoType.Symbol || ModeInfoType == ModeInfoType.MotorBias
                || ModeInfoType == ModeInfoType.CapabilityBits || ModeInfoType == ModeInfoType.ValueFormat)
                modeSpecific = $"{Environment.NewLine}\tValue: {Value}";
            else if (ModeInfoType == ModeInfoType.Raw || ModeInfoType == ModeInfoType.Percent || ModeInfoType == ModeInfoType.Si)
                modeSpecific = $"{Environment.NewLine}\tMinValue: {MinValue}; MaxValue: {MaxValue}";
            else if (ModeInfoType == ModeInfoType.Mapping)
                modeSpecific = $"{Environment.NewLine}\tInputSideMappings: {string.Join(", ", InputSideMappings.Select(c => c.ToString()))}" +
                    $"{Environment.NewLine}\tOutputSideMappings: {string.Join(", ", OutputSideMappings.Select(c => c.ToString()))}";
            var footer = $"{Environment.NewLine}\t[{Body}]";
            return header + modeSpecific + footer;
        }

        private static string ExtractTextValue(string body, int startLocation, string terminator)
        {
            var index = startLocation;
            var byteCount = 0;
            var nextByte = body.Substring(index, 2);
            while (nextByte != terminator)
            {
                index += 2;
                byteCount++;
                nextByte = body.Substring(index, 2);
            }
            var data = DataConverter.HexStringToByteArray(body.Substring(12, byteCount * 2));
            return Encoding.ASCII.GetString(data);
        }

        private static int ExtractFloatAsInt(string body, int startLocation, int length)
        {
            byte[] minBytes = DataConverter.HexStringToByteArray(body.Substring(startLocation, length));
            return (int)BitConverter.ToSingle(minBytes, 0);
        }

        private static IEnumerable<PortModeInfoMappingFlag> ExtractMappingFlags(string body, int startLocation, int length)
        {
            var mappings = new List<PortModeInfoMappingFlag>();
            var inputSideMappingBitfield = (PortModeInfoMappingFlag)Convert.ToInt32(body.Substring(startLocation, length), 16);
            foreach (var value in Enum.GetValues(typeof(PortModeInfoMappingFlag)))
            {
                var mapping = (PortModeInfoMappingFlag)Enum.Parse(typeof(PortModeInfoMappingFlag), value.ToString());
                if ((inputSideMappingBitfield & mapping) == mapping)
                    mappings.Add(mapping);
            }
            return mappings;
        }
    }
}
