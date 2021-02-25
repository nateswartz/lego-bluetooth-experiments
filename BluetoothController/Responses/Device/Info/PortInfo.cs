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
        public List<Capability> Capabilities { get; set; }

        public PortInfo(string body) : base(body)
        {
            Capabilities = new List<Capability>();
            Port = body.Substring(6, 2);
            InfoType = (InformationType)Convert.ToInt32(body.Substring(8, 2), 16);
            var capabilityBitField = (Capability)Convert.ToInt32(body.Substring(10, 2));
            if ((capabilityBitField & Capability.Output) == Capability.Output)
                Capabilities.Add(Capability.Output);
            if ((capabilityBitField & Capability.Input) == Capability.Input)
                Capabilities.Add(Capability.Input);
            if ((capabilityBitField & Capability.LogicalCombinable) == Capability.LogicalCombinable)
                Capabilities.Add(Capability.LogicalCombinable);
            if ((capabilityBitField & Capability.LogicalSynchronizable) == Capability.LogicalSynchronizable)
                Capabilities.Add(Capability.LogicalSynchronizable);
            NotificationType = GetType().Name;
        }

        public override string ToString()
        {
            return $"Port Info ({Port}) " +
                    $"{Enum.GetName(typeof(InformationType), InfoType)}; " +
                    $"Capabilities: {string.Join(", ", Capabilities.Select(c => Enum.GetName(typeof(Capability), c)))} " +
                    $"[{Body}]";
        }
    }
}