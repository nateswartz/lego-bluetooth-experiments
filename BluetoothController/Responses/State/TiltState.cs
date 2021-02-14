namespace BluetoothController.Responses.State
{
    public class TiltState : PortState
    {
        public TiltState(string body) : base(body)
        {
            NotificationType = GetType().Name;
        }

        public override string ToString()
        {
            return $"Tilt Sensor {Event} on port ({Port}) [{Body}]";
        }
    }
}