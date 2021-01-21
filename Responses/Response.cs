using System;

namespace SDKTemplate.Responses
{
    public enum MessageType
    {
        DeviceInfo = 1,
        PortInfo = 4,
        Error = 5,
        SensorData = 69
    }

    public class Response
    {
        public string Body { get; set; }

        public string Length { get; set; }

        public MessageType MessageType { get; set; }

        public Response(string body)
        {
            Body = body;
            Length = body.Substring(0, 2);
            MessageType = (MessageType)Convert.ToInt32(body.Substring(4, 2), 16);
        }

        public override string ToString()
        {
            return Body;
        }
    }
}