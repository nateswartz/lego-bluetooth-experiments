using System;

namespace BluetoothController.Responses
{
    public enum DeviceType
    {
        HubName = 1,
        ButtonState = 2,
        FirmwareVersion = 3,
        SystemType = 11
    }

    public class DeviceInfo : Response
    {
        public DeviceType DeviceType { get; set; }

        public DeviceInfo(string body) : base(body)
        {
            DeviceType = (DeviceType)Convert.ToInt32(body.Substring(6, 2), 16);
            NotificationType = GetType().Name;
        }
    }
}