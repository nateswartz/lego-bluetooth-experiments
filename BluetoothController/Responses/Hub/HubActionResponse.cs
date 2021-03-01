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
        }

        public override string ToString()
        {
            var action = $"performing unknown action {ActionType}";

            switch (ActionType)
            {
                case "31":
                    action = "disconnecting";
                    break;
                case "30":
                    action = "shutting down";
                    break;
            }

            return $"Hub {action} [{Body}]";
        }
    }
}