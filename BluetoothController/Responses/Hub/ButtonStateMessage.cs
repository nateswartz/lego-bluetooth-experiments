using BluetoothController.Models;

namespace BluetoothController.Responses.Hub
{

    public class ButtonStateMessage : HubInfo
    {
        public ButtonState State { get; set; }

        public ButtonStateMessage(string body) : base(body)
        {
            State = Body.Substring(10, 2) switch
            {
                "00" => ButtonState.Released,
                "01" => ButtonState.Pressed,
                _ => ButtonState.Unknown,
            };
        }

        public override string ToString() => $"Button State: {State} [{Body}]";
    }
}