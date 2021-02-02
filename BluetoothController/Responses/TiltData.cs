namespace LegoBoostController.Responses
{
    public class TiltData : SensorData
    {
        public TiltData(string body) : base(body)
        {
        }

        public override string ToString()
        {
            return $"Tilt Sensor Data: {Body}";
        }
    }
}