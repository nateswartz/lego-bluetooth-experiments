namespace BluetoothController.Responses.Device.Info
{
    public class PortInfo : Response
    {
        public string Port { get; set; }

        public PortInfo(string body) : base(body)
        {
            Port = body.Substring(6, 2);

            NotificationType = GetType().Name;
        }

        public override string ToString()
        {
            return $"Port Info ({Port}) [{Body}]";
        }
    }
}