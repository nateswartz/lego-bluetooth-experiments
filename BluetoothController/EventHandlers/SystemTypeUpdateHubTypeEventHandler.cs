using BluetoothController.Controllers;
using BluetoothController.Responses;
using System;
using System.Threading.Tasks;

namespace BluetoothController.EventHandlers
{
    public class SystemTypeUpdateHubTypeEventHandler : IEventHandler
    {
        private readonly HubController _controller;

        public Type HandledEvent { get; } = typeof(SystemType);

        public SystemTypeUpdateHubTypeEventHandler(HubController controller)
        {
            _controller = controller;
        }

        public async Task HandleEventAsync(Response response)
        {
            var data = (SystemType)response;
            _controller.HubType = data.HubType;
            await Task.CompletedTask;
        }
    }
}
