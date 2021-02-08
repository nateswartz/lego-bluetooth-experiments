namespace BluetoothController.Responses
{
    public class Error : Response
    {
        public string ErrorType { get; set; }

        public Error(string body) : base(body)
        {
            var errorType = Body.Substring(8, 2);
            switch (errorType)
            {
                case "05":
                    ErrorType = "Command Not Recognized";
                    break;
                case "06":
                    ErrorType = "Invalid Use";
                    break;
                default:
                    ErrorType = "Unknown";
                    break;
            }
            NotificationType = GetType().Name;
        }

        public override string ToString()
        {
            return $"Error: {ErrorType} (Response Body: {Body})";
        }
    }
}