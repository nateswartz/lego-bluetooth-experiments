using BluetoothController.Models;
using System;

namespace BluetoothController.Responses.Hub
{

    public class HubInfo : Response
    {
        public HubInfoType DeviceType { get; set; }

        public HubInfo(string body) : base(body)
        {
            DeviceType = (HubInfoType)Convert.ToInt32(body.Substring(6, 2), 16);
        }
    }
}