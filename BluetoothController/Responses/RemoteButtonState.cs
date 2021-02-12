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
            return $"Remote Button {Event} on port {PortLetter}({Port}) [{Body}]";
        }
    }
}