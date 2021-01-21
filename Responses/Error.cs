namespace SDKTemplate.Responses
{
    public class Error : Response
    {
        public Error(string body) : base(body)
        {
        }

        public override string ToString()
        {
            return $"Error: Received Unrecognized Command (Response Body: {Body}";
        }
    }
}