namespace SDKTemplate.Responses
{
    public class ColorDistanceState : PortInfo
    {
        public ColorDistanceState(string body) : base(body)
        {
        }

        public override string ToString()
        {
            return $"Color Distance Sensor on port: {PortLetter}({Port})";
        }
    }
}