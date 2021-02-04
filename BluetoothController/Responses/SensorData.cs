namespace BluetoothController.Responses
{
    public class SensorData : Response
    {
        public string Port { get; set; }

        public SensorData(string body) : base(body)
        {
            Port = body.Substring(6, 2);
            NotificationType = GetType().Name;
        }
    }
}