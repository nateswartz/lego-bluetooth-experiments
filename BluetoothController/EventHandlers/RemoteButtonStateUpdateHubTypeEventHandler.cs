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
            if (_controller.Hub != null)
                return;
            _controller.Hub = new RemoteHub();
            await Task.CompletedTask;
        }
    }
}
