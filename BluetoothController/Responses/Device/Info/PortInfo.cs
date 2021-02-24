using System;

namespace BluetoothController.Responses.Device.Info
{
    public enum InformationType
    {
        ModeInfo = 1,
        PossibleModeCombinations = 2
    }

    public class PortInfo : Response
    {
        public string Port { get; set; }
        public InformationType InfoType { get; set; }


        public PortInfo(string body) : base(body)
        {
            Port = body.Substring(6, 2);
            InfoType = (InformationType)Convert.ToInt32(body.Substring(8, 2), 16);

            NotificationType = GetType().Name;
        }

        public override string ToString()
        {
            return $"Port Info ({Port}) {Enum.GetName(typeof(InformationType), InfoType)} [{Body}]";
        }
    }
}