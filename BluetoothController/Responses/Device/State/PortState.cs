using BluetoothController.Models;
using BluetoothController.Models.Enums;
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
            PortLetter = Port switch
            {
                "00" => "A",
                "01" => "B",
                "02" => "C",
                "03" => "D",
                _ => "?",
            };
            if (StateChangeEvent != DeviceState.Detached)
                DeviceType = IOTypes.GetByCode(body.Substring(10, 2));
        }

        public override string ToString()
        {
            return $"{DeviceType?.Name ?? "Device"} {StateChangeEvent} on port {Port} [{Body}]";
        }
    }
}