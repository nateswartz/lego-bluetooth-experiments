using System;
using System.Collections.Generic;

namespace BluetoothController.Responses.Device.State
{
    internal enum FeedbackMessageFlag
    {
        BufferEmptyInProgress = 1,
        BufferEmptyCompleted = 2,
        CurrentCommandDiscarded = 4,
        Idle = 8,
        BusyFull = 16
    }

    public class PortOutputFeedback : Response
    {
        public string Port { get; set; }
        public List<string> Messages { get; set; }

        public PortOutputFeedback(string body) : base(body)
        {
            Port = body.Substring(6, 2);
            Messages = new List<string>();
            var messageBitField = (FeedbackMessageFlag)Convert.ToInt32(body.Substring(8, 2), 16);

            if ((messageBitField & FeedbackMessageFlag.BufferEmptyInProgress) == FeedbackMessageFlag.BufferEmptyInProgress)
                Messages.Add("Buffer Empty + Command In Progress");
            if ((messageBitField & FeedbackMessageFlag.BufferEmptyCompleted) == FeedbackMessageFlag.BufferEmptyCompleted)
                Messages.Add("Buffer Empty + Command Completed");
            if ((messageBitField & FeedbackMessageFlag.CurrentCommandDiscarded) == FeedbackMessageFlag.CurrentCommandDiscarded)
                Messages.Add("Current Command(s) Discarded");
            if ((messageBitField & FeedbackMessageFlag.Idle) == FeedbackMessageFlag.Idle)
                Messages.Add("Idle");
            if ((messageBitField & FeedbackMessageFlag.BusyFull) == FeedbackMessageFlag.BusyFull)
                Messages.Add("Busy/Full");

            NotificationType = GetType().Name;
        }

        public override string ToString()
        {
            return $"Port Feeback for Port {Port} - {string.Join(", ", Messages.ToArray())} [{Body}]";
        }
    }
}