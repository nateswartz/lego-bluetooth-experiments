namespace LegoBoostController.Responses
{
    public class LEDState : PortInfo
    {
        public LEDState(string body) : base(body)
        {
        }

        public override string ToString()
        {
            return "LED Notification: " + Body;
        }
    }
}