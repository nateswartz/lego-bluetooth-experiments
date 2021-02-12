namespace BluetoothController.Responses
{
    public class ExternalMotorState : PortInfo
    {
        public ExternalMotorState(string body) : base(body)
        {
            NotificationType = GetType().Name;
        }

        public override string ToString()
        {
            return $"External Motor {Event} on port {PortLetter}({Port}) [{Body}]";
        }
    }
}