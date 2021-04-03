using BluetoothController.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BluetoothController.Responses.Device.Info
{

    public class PortInfo : Response
    {
        public string Port { get; set; }
        public InformationType InfoType { get; set; }
        public IEnumerable<Capability> Capabilities { get; set; }
        public int TotalModeCount { get; set; }
        public IEnumerable<int> InputModes { get; set; }
        public IEnumerable<int> OutputModes { get; set; }
        public IEnumerable<string> ModeCombinations { get; set; }

        public PortInfo(string body) : base(body)
        {
            Port = body.Substring(6, 2);
            InfoType = (InformationType)Convert.ToInt32(body.Substring(8, 2), 16);
            if (InfoType == InformationType.ModeInfo)
            {
                ExtractModeInfo(body);
            }
            else if (InfoType == InformationType.PossibleModeCombinations)
            {
                ModeCombinations = ExtractPossibleModeCombinations(body);
            }
        }

        private void ExtractModeInfo(string body)
        {
            Capabilities = ExtractCapabilities(body.Substring(10, 2));
            TotalModeCount = Convert.ToInt32(body.Substring(12, 2), 16);
            InputModes = ExtractModes(body.Substring(14, 4));
            OutputModes = ExtractModes(body.Substring(18, 4));
        }

        private static IEnumerable<Capability> ExtractCapabilities(string capabilitySection)
        {
            var capabilities = new List<Capability>();
            var capabilityBitField = (Capability)Convert.ToInt32(capabilitySection, 16);
            foreach (var value in Enum.GetValues(typeof(Capability)))
            {
                var capability = (Capability)Enum.Parse(typeof(Capability), value.ToString());
                if ((capabilityBitField & capability) == capability)
                    capabilities.Add(capability);
            }
            return capabilities;
        }

        private static IEnumerable<int> ExtractModes(string modesSection)
        {
            var modes = new List<int>();
            var inputModesBitField = Convert.ToInt32(modesSection, 16);
            for (var bit = 1; bit <= Math.Pow(2.0, 15.0); bit *= 2)
            {
                if ((inputModesBitField & bit) == bit)
                    modes.Add(Convert.ToInt32(Math.Log(Convert.ToDouble(bit), 2.0)));
            }
            return modes;
        }

        private static IEnumerable<string> ExtractPossibleModeCombinations(string body)
        {
            var modeCombinations = new List<string>();
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
                    modeCombinations.Add($"{readableCombo.Trim()}]");
                    index += 2;
                    combination = Convert.ToInt32(body.Substring(index, 2), 16);
                }
            }
            return modeCombinations;
        }

        public override string ToString()
        {
            var header = $"Port Info ({Port}) " +
                            $"{InfoType}; ";
            var modeSpecific = "";
            if (InfoType == InformationType.ModeInfo)
                modeSpecific = $"{Environment.NewLine}\tCapabilities: {string.Join(", ", Capabilities.Select(c => c.ToString()))} " +
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