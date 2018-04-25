using System.Collections.Generic;

namespace SDKTemplate.Models
{
    public class Motors
    {
        public static Motor A = new Motor("A", 37);
        public static Motor B = new Motor("B", 38);
        public static Motor A_B = new Motor("A+B", 39);

        public static List<Motor> All = new List<Motor>
        {
            A, B, A_B
        };
    }

    public class Motor
    {
        public string Name { get; set; }
        public int Code { get; set; }

        public Motor(string name, int code)
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
