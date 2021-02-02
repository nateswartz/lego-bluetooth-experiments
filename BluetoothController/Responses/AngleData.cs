namespace BluetoothController.Responses
{
    public class AngleData : ExternalMotorData
    {
        public AngleData(string body) : base(body)
        {
        }

        public override string ToString()
        {
            return $"External Motor Angle Data: {Body}";
        }
    }
}