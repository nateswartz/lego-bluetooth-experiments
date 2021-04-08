using BluetoothController.Models;
using BluetoothController.Models.Enums;
using System.Collections.Generic;
using System.Linq;

namespace BluetoothController.Hubs
{
    public class LegoHub : ILegoHub
    {
        public HubType HubType { get; set; }

        public List<HubPort> Ports { get; set; } = new List<HubPort>();

        public HubPort GetPortByID(string portID)
        {
            return Ports.FirstOrDefault(p => p?.PortID == portID);
        }

        public IEnumerable<HubPort> GetPortsByDeviceType(IOType deviceType)
        {
            return Ports.Where(p => p?.DeviceType.Code == deviceType.Code);
        }
    }
}
