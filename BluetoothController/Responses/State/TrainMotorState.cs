namespace BluetoothController.Responses.State
{
    public class TrainMotorState : PortState
    {
        public TrainMotorState(string body) : base(body)
        {
            NotificationType = GetType().Name;
        }

        public override string ToString()
        {
            return $"Train Motor {Event} on port {PortLetter}({Port}) [{Body}]";
        }
    }
}