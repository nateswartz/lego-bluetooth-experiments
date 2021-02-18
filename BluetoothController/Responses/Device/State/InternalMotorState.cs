namespace BluetoothController.Responses.Device.State
{
    public class InternalMotorState : PortState
    {
        public InternalMotorState(string body) : base(body)
        {
            NotificationType = GetType().Name;
        }

        public override string ToString()
        {
            return $"Internal Motor {Event} on port {PortLetter}({Port}) [{Body}]";
        }
    }
}