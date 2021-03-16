namespace BluetoothController.Responses.Hub
{
    public enum ButtonState
    {
        Released,
        Pressed,
        Unknown,
    }

    public class ButtonStateMessage : HubInfo
    {
        public ButtonState State { get; set; }

        public ButtonStateMessage(string body) : base(body)
        {
            switch (Body.Substring(10, 2))
            {
                case "00":
                    State = ButtonState.Released;
                    break;
                case "01":
                    State = ButtonState.Pressed;
                    break;
                default:
                    State = ButtonState.Unknown;
                    break;
            }
        }

        public override string ToString() => $"Button State: {State} [{Body}]";
    }
}