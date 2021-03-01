namespace BluetoothController.Responses.Device.Info
{

    public class PortModeInfo : Response
    {
        public string Port { get; set; }

        public PortModeInfo(string body) : base(body)
        {
            Port = body.Substring(6, 2);
            NotificationType = GetType().Name;
        }

        public override string ToString() => $"Port Mode Info ({Port}) [{Body}]";
    }
}