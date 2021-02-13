namespace BluetoothController.Responses.State
{
    public class ColorDistanceState : PortState
    {
        public ColorDistanceState(string body) : base(body)
        {
            NotificationType = GetType().Name;
        }

        public override string ToString()
        {
            return $"Color Distance Sensor {Event} on port {PortLetter}({Port}) [{Body}]";
        }
    }
}