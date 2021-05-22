using BluetoothController.Models;

namespace BluetoothController.Responses.Device.Data
{
    public class ColorData : SensorData
    {
        public RgbLightColor Color { get; set; }
        public string Mode { get; set; }

        public ColorData(string body, string mode) : base(body)
        {
            Mode = mode;
            if (Mode == "00")
            {
                Color = RgbLightColors.GetByCode(Body.Substring(8, 2));
            }
        }

        public override string ToString()
        {
            return Mode switch
            {
                "00" => $"Color ({Port}) Data: Color - {Color} [{Body}]",
                _ => $"Color ({Port}) Data: For Unhandled Notification Mode {Mode} [{Body}]"
            };
        }
    }
}