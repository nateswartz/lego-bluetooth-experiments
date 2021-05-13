namespace BluetoothController.Responses.Device.Data
{
    public class DistanceData : SensorData
    {
        public DistanceData(string body) : base(body)
        {
        }

        public override string ToString() => $"Distance ({Port}) Data [{Body}]";
    }
}