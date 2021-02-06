namespace BluetoothController.Responses
{
    public class TrainMotorState : PortInfo
    {
        public TrainMotorState(string body) : base(body)
        {
            NotificationType = GetType().Name;
        }

        public override string ToString()
        {
            return $"Train Motor on port: {PortLetter}({Port})";
        }
    }
}