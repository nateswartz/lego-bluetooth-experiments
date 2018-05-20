using SDKTemplate.Models;
using System;
using System.Text;

namespace SDKTemplate.Responses
{
    public class Response
    {
        public string Body { get; set; }

        public string Length { get; set; }

        public MessageType MessageType { get; set; }

        public Response(string body)
        {
            Body = body;
            Length = body.Substring(0, 2);
            MessageType = (MessageType)Convert.ToInt32(body.Substring(4, 2), 16);
        }

        public override string ToString()
        {
            return Body;
        }
    }

    public class DeviceInfo : Response
    {
        public DeviceType DeviceType { get; set; }

        public DeviceInfo(string body) : base(body)
        {
            DeviceType = (DeviceType)Convert.ToInt32(body.Substring(6, 2), 16);
        }
    }

    public class HubName : DeviceInfo
    {
        public HubName(string body) : base(body)
        {
        }

        public override string ToString()
        {
            var results = "Hub Name: ";
            var data = DataConverter.HexStringToByteArray(Body.Substring(10));
            results += Encoding.ASCII.GetString(data);
            return results;
        }
    }

    public class ButtonState : DeviceInfo
    {
        public ButtonState(string body) : base(body)
        {
        }

        public override string ToString()
        {
            return "Button State: " + (Body.Substring(10, 2) == "00" ? "Released" : "Pressed");
        }
    }

    public class FirmwareVersion : DeviceInfo
    {
        public FirmwareVersion(string body) : base(body)
        {
        }

        public override string ToString()
        {
            var version = Body.Substring(10);
            version = $"{version[6]}.{version[7]}.{version[4]}{version[5]}.{version[2]}{version[3]}{version[0]}{version[1]}";
            return "Firmware Version: " + version;
        }
    }

    public class PortInfo : Response
    {
        public string Port { get; set; }
        public DeviceType DeviceType { get; set; } 

        public PortInfo(string body) : base(body)
        {
            Port = body.Substring(6, 2);
            DeviceType = (DeviceType)Convert.ToInt32(body.Substring(10, 2), 16);
        }
    }

    public class LEDState : PortInfo
    {
        public LEDState(string body) : base(body)
        {
        }

        public override string ToString()
        {
            return "LED Notification: " + Body;
        }
    }

    public class ColorDistanceState : PortInfo
    {
        public ColorDistanceState(string body) : base(body)
        {
        }

        public override string ToString()
        {
            return $"Color Distance Sensor on port: " + (Port == "01" ? "C" : "D");
        }
    }

    public class ExternalMotorState : PortInfo
    {
        public ExternalMotorState(string body) : base(body)
        {
        }

        public override string ToString()
        {
            return "External Motor on port: " + (Port == "01" ? "C" : "D");
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
        public string DataType { get; set; } 

        public ExternalMotorData(string body) : base(body)
        {
            DataType = Length == "08" ? "Angle" : "Speed";
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
        public string Speed { get; set; }

        public SpeedData(string body) : base(body)
        {
            Speed = body.Substring(8, 2);
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

    public enum MessageType
    {
        DeviceInfo = 1,
        PortInfo = 4,
        Error = 5,
        SensorData = 69
    }

    public enum DeviceType
    {
        HubName = 1,
        ButtonState = 2,
        FirmwareVersion = 3,
        LEDState = 23,
        ColorDistanceState = 37,
        ExternalMotorState = 38,
        InternalMotorState = 39
    }
}