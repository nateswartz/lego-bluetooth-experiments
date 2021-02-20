namespace BluetoothController.Responses.Device.State
{
    public class VoltageState : PortState
    {
        public VoltageState(string body) : base(body)
        {
            NotificationType = GetType().Name;
        }

        public override string ToString()
        {
            return $"Voltage Sensor {StateChangeEvent} on port ({Port}) [{Body}]";
        }
    }
}