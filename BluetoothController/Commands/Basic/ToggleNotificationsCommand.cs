﻿using BluetoothController.Commands.Abstract;

namespace BluetoothController.Commands.Basic
{
    public class ToggleNotificationsCommand : PortInputFormatSetupSingleCommandType, IPoweredUpCommand
    {
        public string HexCommand { get; set; }

        public ToggleNotificationsCommand(string portId, bool enableNotifications, string sensorMode)
        {
            var state = enableNotifications ? "01" : "00"; // 01 - On; 00 - Off
            var deltaInterval = "01000000"; // value must change by this interval to trigger a message, protects against jitter
            HexCommand = AddHeader($"{portId}{sensorMode}{deltaInterval}{state}");
        }
    }
}


