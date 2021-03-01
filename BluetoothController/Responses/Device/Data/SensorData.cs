namespace BluetoothController.Responses.Device.Data
{
    public class SensorData : Response
    {
        public string Port { get; set; }

        public SensorData(string body) : base(body)
        {
            Port = body.Substring(6, 2);
        }

        public override string ToString() => $"Sensor Data for unknown device on Port {Port} [{Body}]";

    }
}