namespace BluetoothController.Responses
{
    public class Response
    {
        public string Body { get; set; }

        public string Length { get; set; }

        public string MessageType { get; set; }

        public string NotificationType { get; set; } = "";

        public Response(string body)
        {
            Body = body;
            Length = body.Substring(0, 2);
            MessageType = body.Substring(4, 2);
        }

        public override string ToString()
        {
            return Body;
        }
    }
}