namespace LegoBoostController.Commands.Boost
{
    public class EnableButtonNotificationsCommand : IBoostCommand
    {
        public string HexCommand { get; set; }

        public EnableButtonNotificationsCommand()
        {
            HexCommand = "0500010202";
        }
    }
}


