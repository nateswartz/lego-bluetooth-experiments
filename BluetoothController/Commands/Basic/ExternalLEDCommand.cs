﻿using BluetoothController.Commands.Abstract;
using BluetoothController.Controllers;
using BluetoothController.Models;
using System.Linq;

namespace BluetoothController.Commands.Basic
{
    public class ExternalLEDCommand : PortOutputCommandType
    {
        public ExternalLEDCommand(IHubController controller, int powerPercentage)
        {
            var port = controller.Hub.GetPortsByDeviceType(IOTypes.ExternalLED).First().PortID;
            HexCommand = AddHeader($"{port}115100{powerPercentage:X2}");
        }
    }
}
