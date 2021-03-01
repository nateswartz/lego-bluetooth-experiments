using BluetoothController.Hubs;
using System;

namespace BluetoothController.Responses.Hub
{
    public class SystemType : HubInfo
    {
        public HubType HubType { get; }

        public SystemType(string body) : base(body)
        {
            switch (Body.Substring(10, 2))
            {
                case "40":
                    HubType = HubType.BoostMoveHub;
                    break;
                case "41":
                    HubType = HubType.TwoPortHub;
                    break;
                case "42":
                    HubType = HubType.TwoPortHandset;
                    break;
                default:
                    HubType = HubType.Unknown;
                    break;
            }
        }

        public override string ToString() => $"System Type {Enum.GetName(typeof(HubType), HubType)} [{Body}]";
    }
}