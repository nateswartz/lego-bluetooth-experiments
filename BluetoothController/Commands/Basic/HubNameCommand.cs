﻿namespace BluetoothController.Commands.Basic
{
    public class HubNameCommand : DeviceInfoCommand, IPoweredUpCommand
    {
        public string HexCommand { get; set; }

        public HubNameCommand()
        {
            var infoType = "01"; // Name
            var action = "05"; // One-time request
            HexCommand = AddHeader($"{infoType}{action}01");
        }
    }
}

