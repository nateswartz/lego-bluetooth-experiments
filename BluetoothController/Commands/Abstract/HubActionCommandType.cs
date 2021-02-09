using BluetoothController.Models;

namespace BluetoothController.Commands.Abstract
{
    public abstract class HubActionCommandType : CommandType
    {
        public HubActionCommandType() : base(CommandTypes.HubAction)
        {
        }
    }
}


