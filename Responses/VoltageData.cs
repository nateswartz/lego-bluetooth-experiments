using System;

namespace LegoBoostController.Responses
{
    public class VoltageData : SensorData
    {
        public int Voltage { get; set; }

        public VoltageData(string body) : base(body)
        {
            Voltage = Convert.ToInt32($"{body.Substring(10, 2)}{body.Substring(8, 2)}", 16);
        }

        public override string ToString()
        {
            return $"Voltage Sensor Data ({Port}) : {Voltage} - {Body}";
        }
    }
}