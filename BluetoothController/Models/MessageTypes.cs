using System.Collections.Generic;
using System.Linq;

namespace BluetoothController.Models
{
    public class MessageType
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
        public static MessageType HubProperty = new MessageType("01");
        public static MessageType HubAction = new MessageType("02");
        public static MessageType HubAttachedDetachedIO = new MessageType("04");
        public static MessageType Error = new MessageType("05");
        public static MessageType PortInformation = new MessageType("21");
        public static MessageType PortInputFormatSetupSingle = new MessageType("41");
        public static MessageType PortInputFormatSetupCombined = new MessageType("42");
        public static MessageType PortValueSingle = new MessageType("45");
        public static MessageType PortInputFormatSingle = new MessageType("47");
        public static MessageType PortOutput = new MessageType("81");
        public static MessageType PortOutputFeedback = new MessageType("82");

        private static List<MessageType> All = new List<MessageType>
        {
            HubProperty, HubAction, HubAttachedDetachedIO, Error, PortInformation, PortInputFormatSetupSingle, PortInputFormatSetupCombined,
            PortValueSingle, PortInputFormatSingle, PortOutput, PortOutputFeedback
        };

        public static MessageType GetByCode(string code)
        {
            var message = All.Where(c => c.Code.ToLower() == code.ToLower())
                      .FirstOrDefault();
            return message == null ? new MessageType(code) : message;
        }
    }
}
