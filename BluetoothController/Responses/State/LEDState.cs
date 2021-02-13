namespace BluetoothController.Responses.State
{
    public class LEDState : PortState
    {
        public LEDState(string body) : base(body)
        {
            NotificationType = GetType().Name;
        }

        public override string ToString()
        {
            return $"LED {Event} on port {Port} [{Body}]";
        }
    }
}