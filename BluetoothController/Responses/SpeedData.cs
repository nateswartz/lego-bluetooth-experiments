using System;

namespace BluetoothController.Responses
{
    public class SpeedData : SensorData
    {
        public int Speed { get; set; }

        public SpeedData(string body) : base(body)
        {
            Speed = Convert.ToInt32(body.Substring(8, 2), 16);
        }

        public override string ToString()
        {
            return $"External Motor Speed Data: Speed = {Speed} - {Body}";
        }
    }
}