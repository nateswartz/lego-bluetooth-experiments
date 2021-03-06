﻿using BluetoothController.Models.Enums;
using System;
using System.Collections.Generic;

namespace BluetoothController.Responses.Device.State
{

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
        }

        public override string ToString()
        {
            return $"Port Feeback ({Port}) - {string.Join(", ", Messages.ToArray())} [{Body}]";
        }
    }
}