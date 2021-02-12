namespace BluetoothController.Responses
{
    public class InternalMotorState : PortInfo
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