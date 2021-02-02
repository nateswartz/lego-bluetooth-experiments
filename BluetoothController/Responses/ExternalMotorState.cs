namespace BluetoothController.Responses
{
    public class ExternalMotorState : PortInfo
    {
        public ExternalMotorState(string body) : base(body)
        {
        }

        public override string ToString()
        {
            return $"External Motor on port: {PortLetter}({Port})";
        }
    }
}