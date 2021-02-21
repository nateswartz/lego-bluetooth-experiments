using System.Collections.Generic;
using System.Linq;

namespace BluetoothController.Models
{
    public class IOType
    {
        public string Code { get; set; }

        public IOType(string code)
        {
            Code = code;
        }
    }

    public static class IOTypes
    {
        public static IOType None = new IOType("");
        public static IOType TrainMotor = new IOType("02");
        public static IOType VoltageSensor = new IOType("14");
        public static IOType CurrentSensor = new IOType("15");
        public static IOType LED = new IOType("17");
        public static IOType ColorDistance = new IOType("25");
        public static IOType ExternalMotor = new IOType("26");
        public static IOType InternalMotor = new IOType("27");
        public static IOType TiltSensor = new IOType("28");
        public static IOType RemoteButton = new IOType("37");

        public static List<IOType> All = new List<IOType>
        {
            TrainMotor, VoltageSensor, CurrentSensor, LED, ColorDistance, ExternalMotor, InternalMotor, TiltSensor, RemoteButton
        };

        public static IOType GetByCode(string code)
        {
            return All.Where(c => c.Code.ToLower() == code.ToLower())
                      .First();
        }
    }
}