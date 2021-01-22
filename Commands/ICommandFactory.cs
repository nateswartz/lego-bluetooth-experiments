namespace SDKTemplate.Commands
{
    public interface ICommandFactory
    {
        ICommand GetCommand(string keyword);
    }
}