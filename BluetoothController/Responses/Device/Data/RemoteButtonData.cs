using BluetoothController.Models.Enums;
using System;

namespace BluetoothController.Responses.Device.Data
{

    public class RemoteButtonData : SensorData
    {
        public bool PlusPressed { get; set; }
        public bool RedPressed { get; set; }
        public bool MinusPressed { get; set; }

        public RemoteButtonData(string body) : base(body)
        {
            var buttonState = (RemoteButtonFlag)Convert.ToInt32(body.Substring(8, 2), 16);
            PlusPressed = (buttonState & RemoteButtonFlag.Plus) == RemoteButtonFlag.Plus;
            RedPressed = (buttonState & RemoteButtonFlag.Red) == RemoteButtonFlag.Red;
            MinusPressed = (buttonState & RemoteButtonFlag.Minus) == RemoteButtonFlag.Minus;
        }

        public override string ToString()
        {
            return $"Remote Button ({Port}) Data: {(PlusPressed ? "+" : "")}{(RedPressed ? "R" : "")}{(MinusPressed ? "-" : "")} [{Body}]";
        }
    }
}