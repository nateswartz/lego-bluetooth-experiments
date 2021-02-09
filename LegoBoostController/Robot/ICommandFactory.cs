namespace LegoBoostController.Robot
{
    public interface ICommandFactory
    {
        IRobotCommand GetCommand(string keyword);

        string GetCommandExamples();
    }
}