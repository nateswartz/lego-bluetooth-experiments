using System;

namespace BluetoothController.Responses
{
    public static class IOType
    {
        public const string TrainMotor = "02";
        public const string LED = "17";
        public const string ColorDistance = "25";
        public const string ExternalMotor = "26";
        public const string InternalMotor = "27";
        public const string RemoteButton = "37";
    }

    public enum DeviceState
    {
        Detached,
        Attached,
        AttachedVirtual
    }

    public class PortInfo : Response
    {
        public string Port { get; set; }
        public string PortLetter { get; set; }
        public string DeviceType { get; set; }
        public DeviceState Event { get; set; }

        public PortInfo(string body) : base(body)
        {
            Port = body.Substring(6, 2);
            Event = (DeviceState)Convert.ToInt32(body.Substring(8, 2), 16);
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
            if (Event != DeviceState.Detached)
                DeviceType = body.Substring(10, 2);
            NotificationType = GetType().Name;
        }

        public override string ToString()
        {
            return $"Unknown Device {Event} on port {Port} [{Body}]";
        }
    }
}