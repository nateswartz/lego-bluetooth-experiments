using BluetoothController.Models;
using System;

namespace BluetoothController.Responses
{
    public class ColorDistanceData : SensorData
    {
        public LEDColor Color { get; set; }
        public int Inches { get; set; }

        public ColorDistanceData(string body) : base(body)
        {
            var colorCode = Body.Substring(8, 2);
            Color = LEDColors.GetByCode(colorCode);
            Inches = Convert.ToInt32(Body.Substring(10, 2), 16);
        }

        public override string ToString()
        {
            return $"Color/Distance Sensor Data: Color - {Color}; Inches - {Inches}: {Body}";
        }
    }
}