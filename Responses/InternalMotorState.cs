namespace LegoBoostController.Responses
{
    public class InternalMotorState : PortInfo
    {
        public InternalMotorState(string body) : base(body)
        {
        }

        public override string ToString()
        {
            return $"Internal Motor notification: {Body}";
        }
    }
}