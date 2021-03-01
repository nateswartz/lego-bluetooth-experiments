﻿using BluetoothController.Models;
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
            if (Mode == "00")
                return $"Color/Distance ({Port}) Data: Color - {Color} [{Body}]";
            else if (Mode == "01")
                return $"Color/Distance ({Port}) Data: Inches - {Inches} [{Body}]";
            else if (Mode == "02")
                return $"Color/Distance ({Port}) Data: ProximityCounter - {ProximityCounter} [{Body}]";
            else if (Mode == "08")
                return $"Color/Distance ({Port}) Data: Color - {Color}; Inches - {Inches} [{Body}]";
            else
                return $"Color/Distance ({Port}) Data: For Unhandled Notification Mode {Mode} [{Body}]";
        }
    }
}