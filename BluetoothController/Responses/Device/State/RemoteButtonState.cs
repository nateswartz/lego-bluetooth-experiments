namespace BluetoothController.Responses.Device.State
{
    public class RemoteButtonState : PortState
    {
        public RemoteButtonState(string body) : base(body)
        {
            NotificationType = GetType().Name;
        }

        public override string ToString()
        {
            return $"Remote Button {StateChangeEvent} on port {PortLetter}({Port}) [{Body}]";
        }
    }
}