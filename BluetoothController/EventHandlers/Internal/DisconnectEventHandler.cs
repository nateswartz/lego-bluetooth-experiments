using BluetoothController.Controllers;
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

        public async Task HandleEventAsync(HubActionResponse response)
        {
            if (response.ActionType == HubActionTypes.Disconnect || response.ActionType == HubActionTypes.Shutdown)
            {
                await _onDisconnectCallback(_controller);
            }
        }
    }
}
