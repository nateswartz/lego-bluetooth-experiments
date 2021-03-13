using BluetoothController.Controllers;
using BluetoothController.Responses;
using BluetoothController.Responses.Hub;
using System;
using System.Threading.Tasks;

namespace BluetoothController.EventHandlers
{
    internal class SystemTypeUpdateHubTypeEventHandler : EventHandlerBase, IEventHandler
    {
        public Type HandledEvent { get; } = typeof(SystemType);

        public SystemTypeUpdateHubTypeEventHandler(IHubController controller) : base(controller) { }

        public async Task HandleEventAsync(Response response)
        {
            var data = (SystemType)response;
            _controller.Hub.HubType = data.HubType;
            await Task.CompletedTask;
        }
    }
}
