using System;

namespace BluetoothController.Responses.Data
{
    public class SpeedData : SensorData
    {
        public int Speed { get; set; }

        public SpeedData(string body) : base(body)
        {
            Speed = Convert.ToInt32(body.Substring(8, 2), 16);
            NotificationType = GetType().Name;
        }

        public override string ToString()
        {
            return $"External Motor ({Port}) Speed Data: {(Speed == 255 ? 0 : Speed)}";
        }
    }
}