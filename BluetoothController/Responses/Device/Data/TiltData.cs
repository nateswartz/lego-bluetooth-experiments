namespace BluetoothController.Responses.Device.Data
{
    public class TiltData : SensorData
    {
        public TiltData(string body) : base(body)
        {
            NotificationType = GetType().Name;
        }

        public override string ToString() => $"Tilt Data ({Port}) [{Body}]";
    }
}