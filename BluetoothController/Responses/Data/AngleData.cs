namespace BluetoothController.Responses.Data
{
    public class AngleData : ExternalMotorData
    {
        public AngleData(string body) : base(body)
        {
            NotificationType = GetType().Name;
        }

        public override string ToString()
        {
            return $"External Motor Angle Data: {Body}";
        }
    }
}