using System.Collections.Generic;
using System.Linq;

namespace BluetoothController.Models
{
    public record IOType
    {
        public string Code { get; set; }
        public string Name { get; set; }

        public IOType(string code, string name)
        {
            Code = code;
            Name = name;
        }
        public override string ToString()
        {
            return Name;
        }
    }

    public static class IOTypes
    {
        public static readonly IOType None = new("", "");
        public static readonly IOType TrainMotor = new("02", "Train Motor");
        public static readonly IOType ExternalLED = new("08", "External LED");
        public static readonly IOType VoltageSensor = new("14", "Voltage Sensor");
        public static readonly IOType CurrentSensor = new("15", "Current Sensor");
        public static readonly IOType LED = new("17", "LED");
        public static readonly IOType ColorDistance = new("25", "Color/Distance Sensor");
        public static readonly IOType ExternalMotor = new("26", "External Motor");
        public static readonly IOType InternalMotor = new("27", "Internal Motor");
        public static readonly IOType TiltSensor = new("28", "Tilt Sensor");
        public static readonly IOType RemoteButton = new("37", "Remote Button");

        private readonly static List<IOType> _all = new()
        {
            TrainMotor,
            ExternalLED,
            VoltageSensor,
            CurrentSensor,
            LED,
            ColorDistance,
            ExternalMotor,
            InternalMotor,
            TiltSensor,
            RemoteButton
        };

        public static IOType GetByCode(string code)
        {
            return _all.Where(c => c.Code.ToLower() == code.ToLower())
                       .FirstOrDefault()
                       ?? new IOType(code, "");
        }
    }
}