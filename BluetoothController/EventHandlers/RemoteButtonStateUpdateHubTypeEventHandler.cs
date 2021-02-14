using BluetoothController.Controllers;
using BluetoothController.Hubs;
using BluetoothController.Responses;
using BluetoothController.Responses.State;
using System;
using System.Threading.Tasks;

namespace BluetoothController.EventHandlers
{
    public class RemoteButtonStateUpdateHubTypeEventHandler : IEventHandler
    {
        private readonly HubController _controller;

        public Type HandledEvent { get; } = typeof(RemoteButtonState);

        public RemoteButtonStateUpdateHubTypeEventHandler(HubController controller)
        {
            _controller = controller;
        }

        public async Task HandleEventAsync(Response response)
        {
            if (_controller.Hub is RemoteHub)
                return;
            var remoteHub = new RemoteHub();
            if (_controller.Hub != null)
                remoteHub.Ports = _controller.Hub.Ports;
            _controller.Hub = remoteHub;
            await Task.CompletedTask;
        }
    }
}
