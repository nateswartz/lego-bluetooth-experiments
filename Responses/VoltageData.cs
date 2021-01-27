namespace LegoBoostController.Responses
{
    public class VoltageData : SensorData
    {
        public VoltageData(string body) : base(body)
        {
            NotificationType = GetType().Name;
        }

        public override string ToString()
        {
            return $"Voltage Sensor Data ({Port}) : {Body}";
        }
    }
}