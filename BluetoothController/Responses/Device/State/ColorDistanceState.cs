namespace BluetoothController.Responses.Device.State
{
    public class ColorDistanceState : PortState
    {
        public ColorDistanceState(string body) : base(body)
        {
        }

        public override string ToString()
        {
            return $"Color Distance Sensor {StateChangeEvent} on port {PortLetter}({Port}) [{Body}]";
        }
    }
}