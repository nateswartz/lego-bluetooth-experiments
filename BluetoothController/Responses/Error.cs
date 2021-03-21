using BluetoothController.Models;

namespace BluetoothController.Responses
{
    public class Error : Response
    {
        public string FailedCommandType { get; set; }
        public string ErrorType { get; set; }

        public Error(string body) : base(body)
        {
            var failedCommandType = MessageTypes.GetByCode(Body.Substring(6, 2));

            if (failedCommandType == MessageTypes.HubProperty)
                FailedCommandType = "Device Info";
            if (failedCommandType == MessageTypes.HubAction)
                FailedCommandType = "Hub Action";

            var errorType = Body.Substring(8, 2);
            ErrorType = errorType switch
            {
                "05" => "Command Not Recognized",
                "06" => "Invalid Use",
                _ => "Unknown",
            };
            NotificationType = GetType().Name;
        }

        public override string ToString() => $"Error: {ErrorType} (Response Body: {Body})";
    }
}