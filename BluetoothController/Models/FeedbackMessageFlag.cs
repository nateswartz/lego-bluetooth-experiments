namespace BluetoothController.Models
{
    internal enum FeedbackMessageFlag
    {
        BufferEmptyInProgress = 1,
        BufferEmptyCompleted = 2,
        CurrentCommandDiscarded = 4,
        Idle = 8,
        BusyFull = 16
    }
}