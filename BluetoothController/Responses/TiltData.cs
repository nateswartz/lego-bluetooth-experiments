namespace BluetoothController.Responses
{
    public class TiltData : SensorData
    {
        public TiltData(string body) : base(body)
        {
            NotificationType = GetType().Name;
        }

        public override string ToString()
        {
            return $"Tilt Sensor Data: {Body}";
        }
    }
}