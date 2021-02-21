using BluetoothController.Models;
using System.Collections.Generic;
using System.Linq;

namespace BluetoothController.Hubs
{
    public class LegoHub
    {
        public HubType HubType;

        public List<HubPort> Ports = new List<HubPort>();

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
