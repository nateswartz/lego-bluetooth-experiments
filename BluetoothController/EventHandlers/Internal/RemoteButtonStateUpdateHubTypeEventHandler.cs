using BluetoothController.Controllers;
using BluetoothController.Models;
using BluetoothController.Models.Enums;
using BluetoothController.Responses;
using BluetoothController.Responses.Device.State;
using System.Threading.Tasks;

namespace BluetoothController.EventHandlers.Internal
{
    internal class RemoteButtonStateUpdateHubTypeEventHandler : EventHandlerBase, IEventHandler<PortState>
    {
        public RemoteButtonStateUpdateHubTypeEventHandler(IHubController controller) : base(controller) { }

        public async Task<bool> HandleEventAsync(Response response)
        {
            var portState = (PortState)response;
            if (portState.DeviceType == IOTypes.RemoteButton)
            {
                _controller.Hub.HubType = HubType.TwoPortHandset;
                await Task.CompletedTask;
                return true;
            }
            return false;
        }
    }
}
