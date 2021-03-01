namespace BluetoothController.Responses.Device.Data
{
    public class TiltData : SensorData
    {
        public TiltData(string body) : base(body)
        {
        }

        public override string ToString() => $"Tilt Data ({Port}) [{Body}]";
    }
}