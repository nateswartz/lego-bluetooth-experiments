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
            var action = ActionType switch
            {
                "31" => "disconnecting",
                "30" => "shutting down",
                _ => $"performing unknown action {ActionType}"
            };

            return $"Hub {action} [{Body}]";
        }
    }
}