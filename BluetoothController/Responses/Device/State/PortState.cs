using BluetoothController.Models;
using System;

namespace BluetoothController.Responses.Device.State
{
    public class PortState : Response
    {
        public string Port { get; set; }
        public string PortLetter { get; set; }
        public IOType DeviceType { get; set; }
        public DeviceState StateChangeEvent { get; set; }

        public PortState(string body) : base(body)
        {
            Port = body.Substring(6, 2);
            StateChangeEvent = (DeviceState)Convert.ToInt32(body.Substring(8, 2), 16);
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
            if (StateChangeEvent != DeviceState.Detached)
                DeviceType = IOTypes.GetByCode(body.Substring(10, 2));
        }

        public override string ToString() => $"Unknown Device {StateChangeEvent} on port {Port} [{Body}]";
    }
}