using System.Collections.Generic;
using System.Linq;

namespace BluetoothController.Hubs
{
    public class TwoPortHub : HubWithChangeablePorts
    {
        public HubPort PortA;
        public HubPort PortB;

        public TwoPortHub()
        {
            HubType = HubType.TwoPortHub;
            ChangeablePorts = new List<HubPort> { PortA, PortB };
            PortA = new HubPort
            {
                PortID = "00",
            };
            PortB = new HubPort
            {
                PortID = "01"
            };
        }
    }

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

    public class HubPort
    {
        public string PortID { get; set; }
        public string DeviceType { get; set; }
    }
}
