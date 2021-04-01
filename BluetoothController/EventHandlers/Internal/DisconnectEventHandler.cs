using BluetoothController.Controllers;
using BluetoothController.Responses;
using BluetoothController.Responses.Hub;
using System;
using System.Threading.Tasks;

namespace BluetoothController.EventHandlers.Internal
{
    internal class DisconnectEventHandler : EventHandlerBase, IEventHandler<HubActionResponse>
    {
        private readonly Func<IHubController, Task> _onDisconnectCallback;

        public DisconnectEventHandler(IHubController controller, Func<IHubController, Task> onDisconnectCallback)
            : base(controller)
        {
            _onDisconnectCallback = onDisconnectCallback;
        }

        public async Task HandleEventAsync(Response response)
        {
            var data = (HubActionResponse)response;
            if (data.ActionType == HubActionTypes.Disconnect || data.ActionType == HubActionTypes.Shutdown)
            {
                await _onDisconnectCallback(_controller);
            }
        }
    }
}
