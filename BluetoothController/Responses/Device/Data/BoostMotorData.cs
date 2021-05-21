using System;

namespace BluetoothController.Responses.Device.Data
{
    public class BoostMotorData : SensorData
    {
        public int Speed { get; set; }
        public string Mode { get; set; }

        public BoostMotorData(string body, string mode) : base(body)
        {
            Mode = mode;
            if (Mode == "01")
            {
                Speed = Convert.ToInt32(body.Substring(8, 2), 16);
            }
        }

        public override string ToString()
        {
            return Mode switch
            {
                "02" => $"Boost Motor ({Port}) Angle Data [{Body}]",
                "01" => $"Boost Motor ({Port}) Speed Data: {(Speed == 255 ? 0 : Speed)} [{Body}]",
                _ => $"Boost Motor ({Port}) Data [{Body}]",
            };
        }
    }
}