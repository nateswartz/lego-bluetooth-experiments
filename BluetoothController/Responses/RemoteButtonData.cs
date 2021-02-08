namespace BluetoothController.Responses
{
    public class RemoteButtonData : SensorData
    {
        public RemoteButtonData(string body) : base(body)
        {
            NotificationType = GetType().Name;
        }

        public override string ToString()
        {
            return $"Remote Button Data: {Body}";
        }
    }
}