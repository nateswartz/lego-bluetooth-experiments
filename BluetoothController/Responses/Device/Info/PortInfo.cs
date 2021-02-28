using System;
using System.Collections.Generic;
using System.Linq;

namespace BluetoothController.Responses.Device.Info
{
    public enum InformationType
    {
        ModeInfo = 1,
        PossibleModeCombinations = 2
    }

    public enum Capability
    {
        Output = 1,
        Input = 2,
        LogicalCombinable = 4,
        LogicalSynchronizable = 8
    }

    public class PortInfo : Response
    {
        public string Port { get; set; }
        public InformationType InfoType { get; set; }
        public List<Capability> Capabilities { get; set; } = new List<Capability>();
        public int TotalModeCount { get; set; }
        public List<int> InputModes { get; set; } = new List<int>();
        public List<int> OutputModes { get; set; } = new List<int>();
        public List<string> ModeCombinations { get; set; } = new List<string>();

        public PortInfo(string body) : base(body)
        {
            Port = body.Substring(6, 2);
            InfoType = (InformationType)Convert.ToInt32(body.Substring(8, 2), 16);
            if (InfoType == InformationType.ModeInfo)
            {
                var capabilityBitField = (Capability)Convert.ToInt32(body.Substring(10, 2), 16);
                foreach (var value in Enum.GetValues(typeof(Capability)))
                {
                    var capability = (Capability)Enum.Parse(typeof(Capability), value.ToString());
                    if ((capabilityBitField & capability) == capability)
                        Capabilities.Add(capability);
                }
                TotalModeCount = Convert.ToInt32(body.Substring(12, 2), 16);
                var inputModesBitField = Convert.ToInt32(body.Substring(14, 4), 16);
                for (var bit = 1; bit <= Math.Pow(2.0, 15.0); bit *= 2)
                {
                    if ((inputModesBitField & bit) == bit)
                        InputModes.Add(Convert.ToInt32(Math.Log(Convert.ToDouble(bit), 2.0)));
                }
                var outputModesBitField = Convert.ToInt32(body.Substring(18, 4), 16);
                for (var bit = 1; bit <= Math.Pow(2.0, 15.0); bit *= 2)
                {
                    if ((outputModesBitField & bit) == bit)
                        OutputModes.Add(Convert.ToInt32(Math.Log(Convert.ToDouble(bit), 2.0)));
                }
            }
            else if (InfoType == InformationType.PossibleModeCombinations)
            {
                if (body.Length > 10)
                {
                    var index = 10;
                    var combination = Convert.ToInt32(body.Substring(index, 2), 16);
                    while (combination != 0)
                    {
                        var readableCombo = "[";
                        for (var bit = 1; bit <= Math.Pow(2.0, 15.0); bit *= 2)
                        {
                            if ((combination & bit) == bit)
                                readableCombo += $"{Convert.ToInt32(Math.Log(Convert.ToDouble(bit), 2.0))} ";
                        }
                        ModeCombinations.Add($"{readableCombo.Trim()}]");
                        index += 2;
                        combination = Convert.ToInt32(body.Substring(index, 2), 16);
                    }
                }
            }

            NotificationType = GetType().Name;
        }

        public override string ToString()
        {
            var header = $"Port Info ({Port}) " +
                            $"{Enum.GetName(typeof(InformationType), InfoType)}; ";
            var modeSpecific = "";
            if (InfoType == InformationType.ModeInfo)
                modeSpecific = $"{Environment.NewLine}\tCapabilities: {string.Join(", ", Capabilities.Select(c => Enum.GetName(typeof(Capability), c)))} " +
                        $"{Environment.NewLine}\tTotal Mode Count: {TotalModeCount}" +
                        $"{Environment.NewLine}\tInputModes: {string.Join(", ", InputModes)} " +
                        $"{Environment.NewLine}\tOutputModes: {string.Join(", ", OutputModes)} ";
            else if (InfoType == InformationType.PossibleModeCombinations)
                modeSpecific = $"{Environment.NewLine}\tModeCombinations: {string.Join(", ", ModeCombinations)} ";
            var footer = $"{Environment.NewLine}\t[{Body}]";
            return header + modeSpecific + footer;
        }
    }
}