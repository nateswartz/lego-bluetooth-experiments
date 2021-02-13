using System;

namespace BluetoothController.Responses.State
{
    public enum ButtonState
    {
        Released = 0,
        Pressed = 1
    }

    public class ButtonStateMessage : DeviceInfo
    {
        public ButtonState State { get; set; }

        public ButtonStateMessage(string body) : base(body)
        {
            State = Body.Substring(10, 2) == "00" ? ButtonState.Released : ButtonState.Pressed;
            NotificationType = GetType().Name;
        }

        public override string ToString() => $"Button State: {Enum.GetName(typeof(ButtonState), State)}";
    }
}