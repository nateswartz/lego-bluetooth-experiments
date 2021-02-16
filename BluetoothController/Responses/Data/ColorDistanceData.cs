using BluetoothController.Models;
using System;

namespace BluetoothController.Responses.Data
{
    public class ColorDistanceData : SensorData
    {
        public LEDColor Color { get; set; }
        public int Inches { get; set; }
        private bool _extended;

        public ColorDistanceData(string body) : base(body)
        {
            if (body.Length > 10)
            {
                Color = LEDColors.GetByCode(Body.Substring(8, 2));
                Inches = Convert.ToInt32(Body.Substring(10, 2), 16);
                _extended = true;
            }
            else
            {
                Inches = Convert.ToInt32(Body.Substring(8, 2), 16);
                _extended = false;
            }
            NotificationType = GetType().Name;
        }

        public override string ToString()
        {
            if (_extended)
                return $"Color/Distance Sensor Data: Color - {Color}; Inches - {Inches}: {Body}";
            else
                return $"Color/Distance Sensor Data: Inches - {Inches}: {Body}";
        }
    }
}