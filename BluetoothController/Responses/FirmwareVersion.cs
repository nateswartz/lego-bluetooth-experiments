namespace BluetoothController.Responses
{
    public class FirmwareVersion : DeviceInfo
    {
        public string Version { get; set; }

        public FirmwareVersion(string body) : base(body)
        {
            var versionSection = Body.Substring(10);
            Version = $"{versionSection[6]}.{versionSection[7]}.{versionSection[4]}{versionSection[5]}.{versionSection[2]}{versionSection[3]}{versionSection[0]}{versionSection[1]}";
            NotificationType = GetType().Name;
        }

        public override string ToString() => $"Firmware Version: {Version}";
    }
}