using BluetoothController.Controllers;
using BluetoothController.Models;
using BluetoothController.Models.Enums;
using BluetoothController.Responses;
using BluetoothController.Responses.Device.State;
using System.Threading.Tasks;

namespace BluetoothController.EventHandlers.Internal
{
    internal class InternalMotorStateUpdateHubTypeEventHandler : EventHandlerBase, IEventHandler<PortState>
    {
        public InternalMotorStateUpdateHubTypeEventHandler(IHubController controller) : base(controller) { }

        public async Task<bool> HandleEventAsync(Response response)
        {
            var portState = (PortState)response;
            if (portState.DeviceType == IOTypes.InternalMotor)
            {
                _controller.Hub.HubType = HubType.BoostMoveHub;
                await Task.CompletedTask;
                return true;
            }
            return false;
        }
    }
}
