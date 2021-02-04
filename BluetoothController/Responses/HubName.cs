using BluetoothController.Util;
using System.Text;

namespace BluetoothController.Responses
{
    public class HubName : DeviceInfo
    {
        public string Name { get; set; }

        public HubName(string body) : base(body)
        {
            var data = DataConverter.HexStringToByteArray(Body.Substring(10));
            Name = Encoding.ASCII.GetString(data);
            NotificationType = GetType().Name;
        }

        public override string ToString() => $"Hub Name: {Name}";
    }
}