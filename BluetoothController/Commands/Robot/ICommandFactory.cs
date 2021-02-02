namespace BluetoothController.Commands.Robot
{
    public interface ICommandFactory
    {
        IRobotCommand GetCommand(string keyword);

        string GetCommandExamples();
    }
}