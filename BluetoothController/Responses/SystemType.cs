namespace BluetoothController.Responses
{
    public class SystemType : DeviceInfo
    {
        public HubType HubType { get; }

        public SystemType(string body) : base(body)
        {
            HubType = Body.Substring(10, 2) == "41" ? HubType.TwoPortHub : HubType.BoostMoveHub;
        }

        public override string ToString() => $"System Type: {Body}";
    }
}