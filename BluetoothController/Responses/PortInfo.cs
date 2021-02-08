using System;

namespace BluetoothController.Responses
{
    public enum IOType
    {
        TrainMotor = 2,
        LED = 23,
        ColorDistance = 37,
        ExternalMotor = 38,
        InternalMotor = 39,
        RemoteButton = 55
    }

    public class PortInfo : Response
    {
        public string Port { get; set; }
        public string PortLetter { get; set; }
        public IOType DeviceType { get; set; }

        public PortInfo(string body) : base(body)
        {
            Port = body.Substring(6, 2);
            DeviceType = (IOType)Convert.ToInt32(body.Substring(10, 2), 16);
            switch (Port)
            {
                case "00":
                    PortLetter = "A";
                    break;
                case "01":
                    PortLetter = "B";
                    break;
                case "02":
                    PortLetter = "C";
                    break;
                case "03":
                    PortLetter = "D";
                    break;
                default:
                    PortLetter = "?";
                    break;
            }
            NotificationType = GetType().Name;
        }

        public override string ToString()
        {
            return $"Unknown Device on port: {PortLetter}({Port}) - {Body}";
        }
    }
}