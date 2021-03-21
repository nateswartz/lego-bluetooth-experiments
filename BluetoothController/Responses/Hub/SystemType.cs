using BluetoothController.Models;

namespace BluetoothController.Responses.Hub
{
    public class SystemType : HubInfo
    {
        public HubType HubType { get; }

        public SystemType(string body) : base(body)
        {
            HubType = Body.Substring(10, 2) switch
            {
                "40" => HubType.BoostMoveHub,
                "41" => HubType.TwoPortHub,
                "42" => HubType.TwoPortHandset,
                _ => HubType.Unknown,
            };
        }

        public override string ToString() => $"System Type {HubType} [{Body}]";
    }
}