using System.Collections.Generic;

namespace BluetoothController.Hubs
{
    public class BoostMoveHub : HubWithChangeablePorts
    {
        public HubPort PortC;
        public HubPort PortD;

        public BoostMoveHub()
        {
            HubType = HubType.TwoPortHub;
            ChangeablePorts = new List<HubPort> { PortC, PortD };
            PortC = new HubPort
            {
                PortID = "02",
            };
            PortD = new HubPort
            {
                PortID = "03"
            };
        }
    }
}
