using BluetoothController.Controllers;
using BluetoothController.Responses;
using BluetoothController.Responses.Hub;
using System.Threading.Tasks;

namespace BluetoothController.EventHandlers.Internal
{
    internal class SystemTypeUpdateHubTypeEventHandler : EventHandlerBase, IEventHandler<SystemType>
    {
        public SystemTypeUpdateHubTypeEventHandler(IHubController controller) : base(controller) { }

        public async Task HandleEventAsync(Response response)
        {
            var data = (SystemType)response;
            _controller.Hub.HubType = data.HubType;
            await Task.CompletedTask;
        }
    }
}
