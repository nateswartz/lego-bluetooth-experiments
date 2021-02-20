namespace BluetoothController.Responses.Device.State
{
    public class ColorDistanceState : PortState
    {
        public ColorDistanceState(string body) : base(body)
        {
            NotificationType = GetType().Name;
        }

        public override string ToString()
        {
            return $"Color Distance Sensor {StateChangeEvent} on port {PortLetter}({Port}) [{Body}]";
        }
    }
}