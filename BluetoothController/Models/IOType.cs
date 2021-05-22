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
        public static readonly IOType LedLight = new("08", "LED Light");
        public static readonly IOType VoltageSensor = new("14", "Voltage Sensor");
        public static readonly IOType CurrentSensor = new("15", "Current Sensor");
        public static readonly IOType RgbLight = new("17", "RGB Light");
        public static readonly IOType ColorDistanceSensor = new("25", "Color/Distance Sensor");
        public static readonly IOType BoostTachoMotor = new("26", "Boost Tacho Motor");
        public static readonly IOType InternalMotor = new("27", "Internal Motor");
        public static readonly IOType TiltSensor = new("28", "Tilt Sensor");
        public static readonly IOType RemoteButton = new("37", "Remote Button");
        public static readonly IOType ColorSensor = new("3d", "Color Sensor");
        public static readonly IOType DistanceSensor = new("3e", "Distance Sensor");
        public static readonly IOType SmallAngularMotor = new("4b", "Small Angular Motor");

        private readonly static List<IOType> _all = new()
        {
            TrainMotor,
            LedLight,
            VoltageSensor,
            CurrentSensor,
            RgbLight,
            ColorDistanceSensor,
            BoostTachoMotor,
            InternalMotor,
            TiltSensor,
            RemoteButton,
            ColorSensor,
            DistanceSensor,
            SmallAngularMotor
        };

        public static IOType GetByCode(string code)
        {
            return _all.Where(c => c.Code.ToLower() == code.ToLower())
                       .FirstOrDefault()
                       ?? new IOType(code, $"Unknown Device ({code})");
        }
    }
}