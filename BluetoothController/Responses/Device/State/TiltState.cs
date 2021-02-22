namespace BluetoothController.Responses.Device.State
{
    public class TiltState : PortState
    {
        public TiltState(string body) : base(body)
        {
            NotificationType = GetType().Name;
        }

        public override string ToString() => $"Tilt Sensor {StateChangeEvent} on port ({Port}) [{Body}]";
    }
}