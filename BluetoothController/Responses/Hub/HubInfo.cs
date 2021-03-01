using System;

namespace BluetoothController.Responses.Hub
{
    public enum HubInfoType
    {
        HubName = 1,
        ButtonState = 2,
        FirmwareVersion = 3,
        SystemType = 11
    }

    public class HubInfo : Response
    {
        public HubInfoType DeviceType { get; set; }

        public HubInfo(string body) : base(body)
        {
            DeviceType = (HubInfoType)Convert.ToInt32(body.Substring(6, 2), 16);
        }
    }
}