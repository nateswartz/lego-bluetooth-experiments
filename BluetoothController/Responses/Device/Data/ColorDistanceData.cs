using BluetoothController.Models;
using System;

namespace BluetoothController.Responses.Device.Data
{
    public class ColorDistanceData : SensorData
    {
        public LEDColor Color { get; set; }
        public int Inches { get; set; }
        public int ProximityCounter { get; set; }
        public string Mode { get; set; }

        public ColorDistanceData(string body, string mode) : base(body)
        {
            Mode = mode;
            if (Mode == "00")
            {
                Color = LEDColors.GetByCode(Body.Substring(8, 2));
            }
            else if (Mode == "01")
            {
                Inches = Convert.ToInt32(Body.Substring(8, 2), 16);
            }
            else if (Mode == "02")
            {
                ProximityCounter = Convert.ToInt32(Body.Substring(8, 2), 16);
            }
            else if (Mode == "08")
            {
                Color = LEDColors.GetByCode(Body.Substring(8, 2));
                Inches = Convert.ToInt32(Body.Substring(10, 2), 16);
            }
        }

        public override string ToString()
        {
            return Mode switch
            {
                "00" => $"Color/Distance ({Port}) Data: Color - {Color} [{Body}]",
                "01" => $"Color/Distance ({Port}) Data: Inches - {Inches} [{Body}]",
                "02" => $"Color/Distance ({Port}) Data: ProximityCounter - {ProximityCounter} [{Body}]",
                "08" => $"Color/Distance ({Port}) Data: Color - {Color}; Inches - {Inches} [{Body}]",
                _ => $"Color/Distance ({Port}) Data: For Unhandled Notification Mode {Mode} [{Body}]"
            };
        }
    }
}