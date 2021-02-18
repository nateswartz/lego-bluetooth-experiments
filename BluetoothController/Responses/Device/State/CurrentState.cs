namespace BluetoothController.Responses.Device.State
{
    public class CurrentState : PortState
    {
        public CurrentState(string body) : base(body)
        {
            NotificationType = GetType().Name;
        }

        public override string ToString()
        {
            return $"Current Sensor {Event} on port ({Port}) [{Body}]";
        }
    }
}