using System;

namespace BluetoothController.Responses.Device.Data
{
    public class TrainMotorData : SensorData
    {
        public int Speed { get; set; }

        public TrainMotorData(string body) : base(body)
        {
            Speed = Convert.ToInt32(body.Substring(8, 2), 16);
            if (Speed == 255)
                Speed = 0;
            NotificationType = GetType().Name;
        }

        public override string ToString()
        {
            return $"Train Motor Speed Percentage: {Speed} [{Body}]";
        }
    }
}