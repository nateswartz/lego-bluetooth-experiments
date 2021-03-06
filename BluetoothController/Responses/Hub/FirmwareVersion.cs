﻿namespace BluetoothController.Responses.Hub
{
    public class FirmwareVersion : HubInfo
    {
        public string Version { get; set; }

        public FirmwareVersion(string body) : base(body)
        {
            var versionSection = Body[10..];
            Version = $"{versionSection[6]}.{versionSection[7]}.{versionSection[4]}{versionSection[5]}.{versionSection[2]}{versionSection[3]}{versionSection[0]}{versionSection[1]}";
        }

        public override string ToString() => $"Firmware Version: {Version} [{Body}]";
    }
}