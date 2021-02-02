using System.Collections.Generic;

namespace BluetoothController.Models
{
    public class Motors
    {
        public static Motor A = new Motor("A", "00");
        public static Motor B = new Motor("B", "01");
        public static Motor A_B = new Motor("A+B", "10");
        public static Motor External = new Motor("External", "00");

        public static List<Motor> All = new List<Motor>
        {
            A, B, A_B, External
        };
    }

    public class Motor
    {
        public string Name { get; set; }
        public string Code { get; set; }

        public Motor(string name, string code)
        {
            Name = name;
            Code = code;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
