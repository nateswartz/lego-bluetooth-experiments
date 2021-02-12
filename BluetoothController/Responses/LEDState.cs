namespace BluetoothController.Responses
{
    public class LEDState : PortInfo
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