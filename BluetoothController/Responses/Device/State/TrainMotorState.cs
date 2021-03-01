namespace BluetoothController.Responses.Device.State
{
    public class TrainMotorState : PortState
    {
        public TrainMotorState(string body) : base(body)
        {
        }

        public override string ToString() => $"Train Motor {StateChangeEvent} on port {PortLetter}({Port}) [{Body}]";
    }
}