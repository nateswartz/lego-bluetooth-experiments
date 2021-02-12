using BluetoothController.Controllers;
using BluetoothController.Hubs;
using BluetoothController.Responses;
using System;
using System.Threading.Tasks;

namespace BluetoothController.EventHandlers
{
    public class InternalMotorStateUpdateHubTypeEventHandler : IEventHandler
    {
        private readonly HubController _controller;

        public Type HandledEvent { get; } = typeof(InternalMotorState);

        public InternalMotorStateUpdateHubTypeEventHandler(HubController controller)
        {
            _controller = controller;
        }

        public async Task HandleEventAsync(Response response)
        {
            if (_controller.Hub != null)
                return;
            _controller.Hub = new BoostMoveHub();
            await Task.CompletedTask;
        }
    }
}
