using BluetoothController.Controllers;
using BluetoothController.Models;
using BluetoothController.Responses;
using BluetoothController.Responses.Device.State;
using System.Threading.Tasks;

namespace BluetoothController.EventHandlers.Internal
{
    internal class RemoteButtonStateUpdateHubTypeEventHandler : EventHandlerBase, IEventHandler<RemoteButtonState>
    {
        public RemoteButtonStateUpdateHubTypeEventHandler(IHubController controller) : base(controller) { }

        public async Task HandleEventAsync(Response response)
        {
            _controller.Hub.HubType = HubType.TwoPortHandset;
            await Task.CompletedTask;
        }
    }
}
