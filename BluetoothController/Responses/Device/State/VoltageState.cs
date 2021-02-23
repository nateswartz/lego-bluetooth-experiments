namespace BluetoothController.Responses.Device.State
{
    public class VoltageState : PortState
    {
        public VoltageState(string body) : base(body)
        {
            NotificationType = GetType().Name;
        }

        public override string ToString() => $"Voltage Sensor {StateChangeEvent} on port ({Port}) [{Body}]";
    }
}