namespace BluetoothController.Responses
{
    public class RemoteButtonState : PortInfo
    {
        public RemoteButtonState(string body) : base(body)
        {
            NotificationType = GetType().Name;
        }

        public override string ToString()
        {
            return $"Remote Button on port: {PortLetter}({Port}) - {Body}";
        }
    }
}