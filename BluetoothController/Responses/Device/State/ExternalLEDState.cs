namespace BluetoothController.Responses.Device.State
{
    public class ExternalLEDState : PortState
    {
        public ExternalLEDState(string body) : base(body)
        {
        }

        public override string ToString() => $"External LED {StateChangeEvent} on port {PortLetter}({Port}) [{Body}]";
    }
}