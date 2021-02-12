using System.Collections.Generic;
using System.Linq;

namespace BluetoothController.Hubs
{
    public class HubWithChangeablePorts : Hub
    {
        public List<HubPort> ChangeablePorts = new List<HubPort>();

        public HubPort GetPortByID(string portID)
        {
            return ChangeablePorts.FirstOrDefault(p => p?.PortID == portID);
        }

        public IEnumerable<HubPort> GetPortsByDeviceType(string deviceType)
        {
            return ChangeablePorts.Where(p => p?.DeviceType == deviceType);
        }
    }
}
