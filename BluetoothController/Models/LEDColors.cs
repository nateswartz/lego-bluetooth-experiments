using System.Collections.Generic;
using System.Linq;

namespace BluetoothController.Models
{
    public record LEDColor
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
        public static readonly LEDColor None = new("None", "00");
        public static readonly LEDColor Pink = new("Pink", "01");
        public static readonly LEDColor Purple = new("Purple", "02");
        public static readonly LEDColor Blue = new("Blue", "03");
        public static readonly LEDColor LightBlue = new("Light Blue", "04");
        public static readonly LEDColor Cyan = new("Cyan", "05");
        public static readonly LEDColor Green = new("Green", "06");
        public static readonly LEDColor Yellow = new("Yellow", "07");
        public static readonly LEDColor Orange = new("Orange", "08");
        public static readonly LEDColor Red = new("Red", "09");
        public static readonly LEDColor White = new("White", "0a");

        public readonly static List<LEDColor> All = new()
        {
            None,
            Pink,
            Purple,
            Blue,
            LightBlue,
            Cyan,
            Green,
            Yellow,
            Orange,
            Red,
            White
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
