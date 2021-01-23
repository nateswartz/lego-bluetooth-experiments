namespace LegoBoostController.Commands
{
    public interface ICommandFactory
    {
        ICommand GetCommand(string keyword);
    }
}