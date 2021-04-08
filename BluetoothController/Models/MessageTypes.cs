using System.Collections.Generic;
using System.Linq;

namespace BluetoothController.Models
{
    public record MessageType
    {
        public string Code { get; set; }

        public MessageType(string code)
        {
            Code = code;
        }

        public override string ToString()
        {
            return Code;
        }
    }

    public static class MessageTypes
    {
        public static readonly MessageType HubProperty = new("01");
        public static readonly MessageType HubAction = new("02");
        public static readonly MessageType HubAttachedDetachedIO = new("04");
        public static readonly MessageType Error = new("05");
        public static readonly MessageType PortInformationRequest = new("21");
        public static readonly MessageType PortModeInformationRequest = new("22");
        public static readonly MessageType PortInputFormatSetupSingle = new("41");
        public static readonly MessageType PortInputFormatSetupCombined = new("42");
        public static readonly MessageType PortInformation = new("43");
        public static readonly MessageType PortModeInformation = new("44");
        public static readonly MessageType PortValueSingle = new("45");
        public static readonly MessageType PortInputFormatSingle = new("47");
        public static readonly MessageType PortOutput = new("81");
        public static readonly MessageType PortOutputFeedback = new("82");

        private static readonly List<MessageType> _all = new()
        {
            HubProperty,
            HubAction,
            HubAttachedDetachedIO,
            Error,
            PortInformationRequest,
            PortModeInformationRequest,
            PortInputFormatSetupSingle,
            PortInputFormatSetupCombined,
            PortInformation,
            PortModeInformation,
            PortValueSingle,
            PortInputFormatSingle,
            PortOutput,
            PortOutputFeedback
        };

        public static MessageType GetByCode(string code)
        {
            return _all.Where(c => c.Code.ToLower() == code.ToLower())
                       .FirstOrDefault()
                       ?? new MessageType(code);
        }
    }
}
