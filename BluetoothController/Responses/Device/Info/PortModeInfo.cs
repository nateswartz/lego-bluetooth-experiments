using BluetoothController.Models;
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
        public List<PortModeInfoMappingFlag> InputSideMappings { get; set; } = new List<PortModeInfoMappingFlag>();
        public List<PortModeInfoMappingFlag> OutputSideMappings { get; set; } = new List<PortModeInfoMappingFlag>();


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
                    var inputSideMappingBitfield = (PortModeInfoMappingFlag)Convert.ToInt32(body.Substring(12, 2), 16);
                    foreach (var value in Enum.GetValues(typeof(PortModeInfoMappingFlag)))
                    {
                        var mapping = (PortModeInfoMappingFlag)Enum.Parse(typeof(PortModeInfoMappingFlag), value.ToString());
                        if ((inputSideMappingBitfield & mapping) == mapping)
                            InputSideMappings.Add(mapping);
                    }
                    var outputSideMappingBitfield = (PortModeInfoMappingFlag)Convert.ToInt32(body.Substring(14, 2), 16);
                    foreach (var value in Enum.GetValues(typeof(PortModeInfoMappingFlag)))
                    {
                        var mapping = (PortModeInfoMappingFlag)Enum.Parse(typeof(PortModeInfoMappingFlag), value.ToString());
                        if ((outputSideMappingBitfield & mapping) == mapping)
                            OutputSideMappings.Add(mapping);
                    }
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
    }
}