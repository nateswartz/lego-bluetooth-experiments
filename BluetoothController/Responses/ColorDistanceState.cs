namespace BluetoothController.Responses
{
    public class ColorDistanceState : PortInfo
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