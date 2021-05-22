using System.Collections.Generic;
using System.Linq;

namespace BluetoothController.Models
{
    public record RgbLightColor
    {
        public string Name { get; set; }
        public string Code { get; set; }

        public RgbLightColor(string name, string code)
        {
            Name = name;
            Code = code;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public static class RgbLightColors
    {
        public static readonly RgbLightColor None = new("None", "00");
        public static readonly RgbLightColor Pink = new("Pink", "01");
        public static readonly RgbLightColor Purple = new("Purple", "02");
        public static readonly RgbLightColor Blue = new("Blue", "03");
        public static readonly RgbLightColor LightBlue = new("Light Blue", "04");
        public static readonly RgbLightColor Cyan = new("Cyan", "05");
        public static readonly RgbLightColor Green = new("Green", "06");
        public static readonly RgbLightColor Yellow = new("Yellow", "07");
        public static readonly RgbLightColor Orange = new("Orange", "08");
        public static readonly RgbLightColor Red = new("Red", "09");
        public static readonly RgbLightColor White = new("White", "0a");

        public readonly static List<RgbLightColor> All = new()
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

        public static RgbLightColor GetByCode(string code)
        {
            if (code == "ff")
            {
                return None;
            }
            return All.Where(c => c.Code.ToLower() == code.ToLower())
                      .First();
        }

        public static RgbLightColor GetByName(string name)
        {
            return All.Where(c => c.Name.ToLower() == name.ToLower())
                      .First();
        }
    }
}
