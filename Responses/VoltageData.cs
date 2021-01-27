using System;

namespace LegoBoostController.Responses
{
    public class VoltageData : SensorData
    {
        public int Voltage { get; set; }

        public VoltageData(string body) : base(body)
        {
            Voltage = Convert.ToInt32(body.Substring(8, 4), 16);
        }

        public override string ToString()
        {
            return $"Voltage Sensor Data ({Port}) : {Voltage} - {Body}";
        }
    }
}