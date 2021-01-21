using System;

namespace SDKTemplate.Responses
{
    public enum MotorDataType
    {
        Angle = 8,
        Speed = 5
    }

    public class ExternalMotorData : SensorData
    {
        public MotorDataType DataType { get; set; }

        public ExternalMotorData(string body) : base(body)
        {
            DataType = (MotorDataType)Convert.ToInt32(Length, 16);
        }

        public override string ToString()
        {
            return $"External Motor Sensor Data: {DataType} - {Body}";
        }
    }
}