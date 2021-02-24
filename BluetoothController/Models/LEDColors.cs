using System.Collections.Generic;
using System.Linq;

namespace BluetoothController.Models
{
    public class LEDColor
    {
        public string Name { get; set; }
        public string Code { get; set; }

        public LEDColor(string name, string code)
        {
            Name = name;
            Code = code;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public static class LEDColors
    {
        public static LEDColor None = new LEDColor("None", "00");
        public static LEDColor Pink = new LEDColor("Pink", "01");
        public static LEDColor Purple = new LEDColor("Purple", "02");
        public static LEDColor Blue = new LEDColor("Blue", "03");
        public static LEDColor LightBlue = new LEDColor("Light Blue", "04");
        public static LEDColor Cyan = new LEDColor("Cyan", "05");
        public static LEDColor Green = new LEDColor("Green", "06");
        public static LEDColor Yellow = new LEDColor("Yellow", "07");
        public static LEDColor Orange = new LEDColor("Orange", "08");
        public static LEDColor Red = new LEDColor("Red", "09");
        public static LEDColor White = new LEDColor("White", "0a");

        public readonly static List<LEDColor> All = new List<LEDColor>
        {
            None, Pink, Purple, Blue, LightBlue, Cyan, Green, Yellow, Orange, Red, White
        };

        public static LEDColor GetByCode(string code)
        {
            if (code == "ff")
            {
                return None;
            }
            return All.Where(c => c.Code.ToLower() == code.ToLower())
                      .First();
        }

        public static LEDColor GetByName(string name)
        {
            return All.Where(c => c.Name.ToLower() == name.ToLower())
                      .First();
        }
    }
}
