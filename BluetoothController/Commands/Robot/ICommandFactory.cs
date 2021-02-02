namespace LegoBoostController.Commands.Robot
{
    public interface ICommandFactory
    {
        IRobotCommand GetCommand(string keyword);

        string GetCommandExamples();
    }
}