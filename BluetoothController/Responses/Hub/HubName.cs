using BluetoothController.Util;
using System.Text;

namespace BluetoothController.Responses.Hub
{
    public class HubName : HubInfo
    {
        public string Name { get; set; }

        public HubName(string body) : base(body)
        {
            var data = DataConverter.HexStringToByteArray(Body[10..]);
            Name = Encoding.ASCII.GetString(data);
        }

        public override string ToString() => $"Hub Name: {Name} [{Body}]";
    }
}