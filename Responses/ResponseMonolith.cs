using SDKTemplate.Models;
using System;

namespace SDKTemplate.Responses
{
    public class ColorDistanceState : PortInfo
    {
        public ColorDistanceState(string body) : base(body)
        {
        }

        public override string ToString()
        {
            return $"Color Distance Sensor on port: {(Port == "02" ? "C" : "D")}({Port})";
        }
    }

    public class ExternalMotorState : PortInfo
    {
        public ExternalMotorState(string body) : base(body)
        {
        }

        public override string ToString()
        {
            return $"External Motor on port: {(Port == "02" ? "C" : "D")}({Port})";
        }
    }

    public class InternalMotorState : PortInfo
    {
        public InternalMotorState(string body) : base(body)
        {
        }

        public override string ToString()
        {
            return "Internal Motor notification: " + Body;
        }
    }

    public class Error : Response
    {
        public Error(string body) : base(body)
        {
        }

        public override string ToString()
        {
            return "Unknown Command (Full Response: " + Body + ")";
        }
    }

    public class SensorData : Response
    {
        public string Port { get; set; }

        public SensorData(string body) : base(body)
        {
            Port = body.Substring(6, 2);
        }
    }

    public class ColorDistanceData : SensorData
    {
        public ColorDistanceData(string body) : base(body)
        {
        }

        public override string ToString()
        {
            var colorCode = Body.Substring(8, 2);
            var color = LEDColors.GetByCode(colorCode);
            int inches = Convert.ToInt32(Body.Substring(10, 2), 16);
            return $"Color/Distance Sensor Data: Color - {color}; Inches - {inches}: {Body}";
        }
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
            return "External Motor Sensor Data: " + DataType + " - " + Body;
        }
    }

    public class AngleData : ExternalMotorData
    {
        public AngleData(string body) : base(body)
        {
        }

        public override string ToString()
        {
            return "External Motor Angle Data: " + Body;
        }
    }

    public class SpeedData : SensorData
    {
        public int Speed { get; set; }

        public SpeedData(string body) : base(body)
        {
            Speed = Convert.ToInt32(body.Substring(8, 2), 16);
        }

        public override string ToString()
        {
            return "External Motor Speed Data: Speed = " + Speed + " - " + Body;
        }
    }

    public class TiltData : SensorData
    {
        public TiltData(string body) : base(body)
        {
        }

        public override string ToString()
        {
            return "Tilt Sensor Data: " + Body;
        }
    }

    public enum MotorDataType
    {
        Angle = 8,
        Speed = 5
    }
}