using BluetoothController.Controllers;
using BluetoothController.Responses.Hub;
using System.Threading.Tasks;

namespace BluetoothController.EventHandlers.Internal
{
    internal class SystemTypeUpdateHubTypeEventHandler : EventHandlerBase, IEventHandler<SystemType>
    {
        public SystemTypeUpdateHubTypeEventHandler(IHubController controller) : base(controller) { }

        public async Task HandleEventAsync(SystemType response)
        {
            _controller.Hub.HubType = response.HubType;
            await Task.CompletedTask;
        }
    }
}
