namespace BluetoothController.Responses.State
{
    public class VoltageState : PortState
    {
        public VoltageState(string body) : base(body)
        {
            NotificationType = GetType().Name;
        }

        public override string ToString()
        {
            return $"Voltage Sensor {Event} on port {PortLetter}({Port}) [{Body}]";
        }
    }
}