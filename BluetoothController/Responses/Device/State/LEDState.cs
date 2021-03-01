namespace BluetoothController.Responses.Device.State
{
    public class LEDState : PortState
    {
        public LEDState(string body) : base(body)
        {
        }

        public override string ToString()
        {
            return $"LED {StateChangeEvent} on port {Port} [{Body}]";
        }
    }
}