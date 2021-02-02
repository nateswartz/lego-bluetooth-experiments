using System;

namespace BluetoothController.Responses
{
    public class PortInfo : Response
    {
        public string Port { get; set; }
        public string PortLetter { get; set; }
        public DeviceType DeviceType { get; set; }

        public PortInfo(string body) : base(body)
        {
            Port = body.Substring(6, 2);
            DeviceType = (DeviceType)Convert.ToInt32(body.Substring(10, 2), 16);
            PortLetter = Port == "02" ? "C" : "D";
        }
    }
}