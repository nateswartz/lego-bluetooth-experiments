using System;

namespace BluetoothController.Responses.Data
{
    public class ExternalMotorData : SensorData
    {
        public int Speed { get; set; }
        public string Mode { get; set; }

        public ExternalMotorData(string body, string mode) : base(body)
        {
            Mode = mode;
            if (mode == "01")
            {
                Speed = Convert.ToInt32(body.Substring(8, 2), 16);
            }

            NotificationType = GetType().Name;
        }

        public override string ToString()
        {
            if (Mode == "02")
                return $"External Motor Angle Data: {Body}";
            if (Mode == "01")
                return $"External Motor ({Port}) Speed Data: {(Speed == 255 ? 0 : Speed)}";
            return $"External Motor Sensor Data: {Body}";
        }
    }
}