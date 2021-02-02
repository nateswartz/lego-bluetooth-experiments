using LegoBoostController.Util;
using System.Text;

namespace LegoBoostController.Responses
{
    public class HubName : DeviceInfo
    {
        public string Name { get; set; }

        public HubName(string body) : base(body)
        {
            var data = DataConverter.HexStringToByteArray(Body.Substring(10));
            Name = Encoding.ASCII.GetString(data);
        }

        public override string ToString() => $"Hub Name: {Name}";
    }
}