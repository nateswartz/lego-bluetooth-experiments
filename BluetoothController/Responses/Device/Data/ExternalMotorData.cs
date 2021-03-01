using System;

namespace BluetoothController.Responses.Device.Data
{
    public class ExternalMotorData : SensorData
    {
        public int Speed { get; set; }
        public string Mode { get; set; }

        public ExternalMotorData(string body, string mode) : base(body)
        {
            Mode = mode;
            if (Mode == "01")
            {
                Speed = Convert.ToInt32(body.Substring(8, 2), 16);
            }
        }

        public override string ToString()
        {
            switch (Mode)
            {
                case "02":
                    return $"External Motor ({Port}) Angle Data [{Body}]";
                case "01":
                    return $"External Motor ({Port}) Speed Data: {(Speed == 255 ? 0 : Speed)} [{Body}]";
                default:
                    return $"External Motor ({Port}) Data [{Body}]";
            }
        }
    }
}