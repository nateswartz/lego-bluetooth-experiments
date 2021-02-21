using System.Collections.Generic;
using System.Linq;

namespace BluetoothController.Models
{
    public class IOType
    {
        public string Name { get; set; }
        public string Code { get; set; }

        public IOType(string name, string code)
        {
            Name = name;
            Code = code;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public static class IOTypes
    {
        public static IOType None = new IOType("None", "");
        public static IOType TrainMotor = new IOType("TrainMotor", "02");
        public static IOType VoltageSensor = new IOType("VoltageSensor", "14");
        public static IOType CurrentSensor = new IOType("CurrentSensor", "15");
        public static IOType LED = new IOType("LED", "17");
        public static IOType ColorDistance = new IOType("ColorDistance", "25");
        public static IOType ExternalMotor = new IOType("ExternalMotor", "26");
        public static IOType InternalMotor = new IOType("InternalMotor", "27");
        public static IOType TiltSensor = new IOType("TiltSensor", "28");
        public static IOType RemoteButton = new IOType("RemoteButton", "37");

        public static List<IOType> All = new List<IOType>
        {
            TrainMotor, VoltageSensor, CurrentSensor, LED, ColorDistance, ExternalMotor, InternalMotor, TiltSensor, RemoteButton
        };

        public static IOType GetByCode(string code)
        {
            return All.Where(c => c.Code.ToLower() == code.ToLower())
                      .First();
        }

        public static IOType GetByName(string name)
        {
            return All.Where(c => c.Name.ToLower() == name.ToLower())
                      .First();
        }
    }
}