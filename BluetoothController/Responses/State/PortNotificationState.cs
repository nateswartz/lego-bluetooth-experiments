namespace BluetoothController.Responses.State
{
    public class PortNotificationState : Response
    {
        public string Port { get; set; }
        public string Mode { get; set; }

        public PortNotificationState(string body) : base(body)
        {
            Port = body.Substring(6, 2);
            Mode = body.Substring(8, 2);

            NotificationType = GetType().Name;
        }

        public override string ToString()
        {
            return $"Port {Port} Registered for notifications with mode {Mode} [{Body}]";
        }
    }
}