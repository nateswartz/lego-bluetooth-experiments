namespace BluetoothController.Responses.Hub
{
    public static class HubActionTypes
    {
        public const string Shutdown = "30";
        public const string Disconnect = "31";
    }

    public class HubActionResponse : Response
    {
        public string ActionType { get; set; }

        public HubActionResponse(string body) : base(body)
        {
            ActionType = body.Substring(6, 2);
            NotificationType = GetType().Name;
        }

        public override string ToString()
        {
            var action = $"performing unknown action {ActionType}";

            if (ActionType == "31")
                action = "disconnecting";
            if (ActionType == "30")
                action = "shutting down";

            return $"Hub {action} [{Body}]";
        }
    }
}