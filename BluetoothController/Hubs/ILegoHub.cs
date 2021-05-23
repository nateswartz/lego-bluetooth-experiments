using BluetoothController.Models;
using BluetoothController.Models.Enums;
using System.Collections.Generic;

namespace BluetoothController.Hubs
{
    public interface ILegoHub
    {
        HubType HubType { get; set; }
        List<HubPort> Ports { get; set; }

        HubPort GetPortByID(string portID);
        IEnumerable<HubPort> GetPortsByDeviceType(IoDeviceType deviceType);
    }
}