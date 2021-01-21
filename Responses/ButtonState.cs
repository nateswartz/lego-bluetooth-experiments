namespace SDKTemplate.Responses
{
    public class ButtonState : DeviceInfo
    {
        public string State { get; set; }

        public ButtonState(string body) : base(body)
        {
            State = (Body.Substring(10, 2) == "00" ? "Released" : "Pressed");
        }

        public override string ToString() => $"Button State: {State}";
    }
}