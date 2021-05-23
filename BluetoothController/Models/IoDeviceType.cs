using System.Collections.Generic;
using System.Linq;

namespace BluetoothController.Models
{
    public record IoDeviceType
    {
        public string Code { get; set; }
        public string Name { get; set; }

        public IoDeviceType(string code, string name)
        {
            Code = code;
            Name = name;
        }
        public override string ToString()
        {
            return Name;
        }
    }

    public static class IoDeviceTypes
    {
        public static readonly IoDeviceType None = new("", "");
        public static readonly IoDeviceType TrainMotor = new("02", "Train Motor");
        public static readonly IoDeviceType LedLight = new("08", "LED Light");
        public static readonly IoDeviceType VoltageSensor = new("14", "Voltage Sensor");
        public static readonly IoDeviceType CurrentSensor = new("15", "Current Sensor");
        public static readonly IoDeviceType RgbLight = new("17", "RGB Light");
        public static readonly IoDeviceType ColorDistanceSensor = new("25", "Color/Distance Sensor");
        public static readonly IoDeviceType BoostTachoMotor = new("26", "Boost Tacho Motor");
        public static readonly IoDeviceType InternalMotor = new("27", "Internal Motor");
        public static readonly IoDeviceType TiltSensor = new("28", "Tilt Sensor");
        public static readonly IoDeviceType RemoteButton = new("37", "Remote Button");
        public static readonly IoDeviceType ColorSensor = new("3d", "Color Sensor");
        public static readonly IoDeviceType DistanceSensor = new("3e", "Distance Sensor");
        public static readonly IoDeviceType SmallAngularMotor = new("4b", "Small Angular Motor");

        private readonly static List<IoDeviceType> _all = new()
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

        public static IoDeviceType GetByCode(string code)
        {
            return _all.Where(c => c.Code.ToLower() == code.ToLower())
                       .FirstOrDefault()
                       ?? new IoDeviceType(code, $"Unknown Device ({code})");
        }
    }
}